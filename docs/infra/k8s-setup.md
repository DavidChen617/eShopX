## k8s 配置(Containerd + Calico + kubeadm)

### 主機名設置
```shell
# 控制節點
hostnamectl set-hostname k8s-control

# Worker 節點
hostnamectl set-hostname k8s-work1
hostnamectl set-hostname k8s-work2
```

### 基礎環境設定

```shell
apt-get update && apt-get upgrade -y
swapoff -a
apt-get install vim -y
```

### 安裝 containerd

```shell
apt-get install containerd -y

mkdir -p /etc/containerd
containerd config default > /etc/containerd/config.toml
```

### 修改 /etc/containerd/config.toml

- 找到以下區塊，修改為：
```toml
[plugins."io.containerd.grpc.v1.cri".containerd.runtimes.runc.options]
  SystemdCgroup = true

[plugins."io.containerd.grpc.v1.cri"]
  sandbox_image = "registry.k8s.io/pause:3.10"
```

### 然後重啟
```shell
systemctl restart containerd
```

### 安裝 Kubernetes 工具
```shell
apt-get install -y apt-transport-https ca-certificates curl gpg

mkdir -p -m 755 /etc/apt/keyrings
curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.32/deb/Release.key \
  | gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg

echo "deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.32/deb/ /" \
  | tee /etc/apt/sources.list.d/kubernetes.list

apt-get update
apt-get install -y kubelet kubeadm kubectl
apt-mark hold kubelet kubeadm kubectl
```

### 啟用 IP Forward
```shell
echo "net.ipv4.ip_forward=1" >> /etc/sysctl.conf
sysctl -p
```

### 初始化控制節點
```shell
kubeadm init --pod-network-cidr=192.168.0.0/16
```

### 設定 kubeconfig：

```shell
mkdir -p $HOME/.kube
cp /etc/kubernetes/admin.conf $HOME/.kube/config
chown $(id -u):$(id -g) $HOME/.kube/config
```

### 安裝 Calico 網路套件

```shell
kubectl create -f https://raw.githubusercontent.com/projectcalico/calico/v3.29.3/manifests/tigera-operator.yaml
curl -O https://raw.githubusercontent.com/projectcalico/calico/v3.29.3/manifests/custom-resources.yaml
kubectl create -f custom-resources.yaml
```

### 加入 Worker 節點

- 在控制節點產生 join 指令：
```shell
kubeadm token create --print-join-command
```

## 查看 calico 狀態

```shell
kubectl get pods -n kube-system | grep calico
kubectl get pods -o wide
```

- 做跨節點連通測試
```bash
kubectl run nginx-control --image=nginx --overrides='{"spec":{"nodeName":"k8s-control"}}'
kubectl run nginx-work1   --image=nginx --overrides='{"spec":{"nodeName":"k8s-work1"}}'
kubectl run nginx-work2   --image=nginx --overrides='{"spec":{"nodeName":"k8s-work2"}}'
```

- 先查各 pod IP
```shell
kubectl get pods -o wide
```

- 從 control curl work1 和 work2
```shell
kubectl exec nginx-control -- curl -s --max-time 5 <work1_ip>
kubectl exec nginx-control -- curl -s --max-time 5 <work2_ip>
```

- 從 work1 curl control 和 work2
```shell
kubectl exec nginx-work1 -- curl -s --max-time 5 <control_ip>
kubectl exec nginx-work1 -- curl -s --max-time 5 <work2_ip>
```

- 從 work2 curl control 和 work1
```shell
kubectl exec nginx-work2 -- curl -s --max-time 5 <control_ip>
kubectl exec nginx-work2 -- curl -s --max-time 5 <work1_ip>
```


### Github runner
```shell
# 前期還有一些指令，可以到 Github Action Runner 上找到
sudo ./svc.sh install
sudo ./svc.sh start
```

### 匯出本地 PostgreSQL Data 並導入至遠端 SQL
```bash
# 本地 docker
docker exec <container_name> pg_dump -U user -d eShopX -F c -f /tmp/eShopX.dump
docker cp <container_name>:/tmp/eShopX.dump ./eShopX.dump
# 複製到本地
scp user@<vm_ip>:~/eShopX.dump ./eShopX.dump

# 複製到 EC2
scp -i eshopx-key.pem ./eShopX.dump ubuntu@<ec2_public_ip>:~/eShopX.dump

# ec2 k8s
kubectl exec -i eshopx-postgres-0 -- bash -c "cat > /var/lib/postgresql/eShopX.dump" < ./eShopX.dump

kubectl exec -it eshopx-postgres-0 -- pg_restore -U user -d eShopX -F c /var/lib/postgresql/eShopX.dump

kubectl exec -it eshopx-postgres-0 -- psql -U user -d eShopX -c "SELECT COUNT(*) FROM \"Users\";"
```

### 確認 EBS
```bash
kubectl get pods -n kube-system | grep ebs
```

### 查 IAM
```bash
aws ec2 describe-instances --filters "Name=tag:Name,Values=*worker*" --query "Reservations[].Instances[].IamInstanceProfile.Arn" --output text

aws iam list-attached-role-policies --role-name eshopx-ebs-csi-role --output table

aws ec2 modify-instance-metadata-options \
    --instance-id <worker-instance-id> \
    --http-put-response-hop-limit 2 \
    --http-endpoint enabled
```

## 安裝 EBS
```bash
kubectl apply -k "github.com/kubernetes-sigs/aws-ebs-csi-driver/deploy/kubernetes/overlays/stable/?ref=release-1.38"
```

### EC2 預設 hop limit = 1，pod 在容器裡多一層網路，導致 EBS CSI driver 拿不到 IAM credentials，CreateVolume 超時。

```bash
# 編輯 CoreDNS configmap
kubectl edit configmap -n kube-system coredns

# 將 Corefile 中：
forward . /etc/resolv.conf {
# 改為：
forward . 169.254.169.253 { # 169.254.169.253 是 AWS link-local DNS

# 重啟 CoreDNS 套用設定
kubectl rollout restart deployment/coredns -n kube-system
# 重啟 EBS CSI controller（如果 PVC 還在 Pending）
kubectl rollout restart deployment/ebs-csi-controller -n kube-system
```

## 安裝 cert-manager
```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.20.0/cert-manager.yaml

kubectl wait --for=condition=Available deployment --all -n cert-manager --timeout=120s
```

## 安裝 Ingress

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.15.0/deploy/static/provider/cloud/deploy.yaml

# hostNetwork + dnsPolicy — pod 直接綁定 host 的 port 80/443
kubectl patch deployment ingress-nginx-controller -n ingress-nginx --patch '{"spec":{"template":{"spec":{"hostNetwork":true,"dnsPolicy":"ClusterFirstWithHostNet"}}}}'

# nodeSelector + toleration — 強制跑在 master 上
kubectl patch deployment ingress-nginx-controller -n ingress-nginx \
    --patch '{
      "spec": {
        "template": {
          "spec": {
            "nodeSelector": {"kubernetes.io/hostname": "k8s-control"},
            "tolerations": [{"key": "node-role.kubernetes.io/control-plane","operator": "Exists","effect": "NoSchedule"}]
          }
        }
      }
    }'
```
