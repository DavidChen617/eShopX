# Troubleshooting Notes (6-8)

## 6. Calico node 不 Ready（BGP/Typha）

- 現象：
  - `calico-node` 長時間 `0/1 Running`
  - log 出現 `BIRD is not ready` 或 `Failed to connect to Typha`
- 原因：
  - BGP 開啟但環境不需要（BIRD 啟動/連線失敗）
  - Typha 連線被安全群組擋住（TCP 5473）
- 解法：
  - 關閉 BGP：
    - `kubectl patch installation default --type=merge -p '{"spec":{"calicoNetwork":{"bgp":"Disabled"}}}'`
  - 安全群組開放 `TCP 5473`（同 SG 互通）

## 7. VXLAN CrossSubnet + BGP disabled → 跨節點 Pod 不通

- 現象：
  - `calico-node` 全部 Ready，但 Pod 跨節點 ping 不通
- 原因：
  - `vxlanMode: CrossSubnet` 在同子網不封裝
  - 又已關閉 BGP，缺乏路由宣告
- 解法：
  - 改成 VXLAN Always（在 Installation 設定 `encapsulation: VXLAN`）
  - 需在 `spec.calicoNetwork.ipPools[]` 補齊必要欄位（至少 `cidr`）

## 8. cert-manager 建立 ClusterIssuer 失敗

- 現象：
  - `no matches for kind "ClusterIssuer" in version "cert-manager.io/v1"`
  - 或 webhook 連線失敗
- 原因：
  - cert-manager CRD 未安裝
  - webhook 尚未 Ready
- 解法：
  - 先安裝 cert-manager：
    - `kubectl apply -f https://github.com/cert-manager/cert-manager/releases/latest/download/cert-manager.yaml`
  - 等 `cert-manager-webhook` Ready 後再建立 `ClusterIssuer`

## 9. gp3 / EBS CSI Driver 無法動態建立 PVC

- 現象：
  - `PVC` 長時間 `Pending`
  - `StorageClass` 使用 `gp3` 但沒有 volume 建立
- 原因：
  - 節點未掛 IAM Role（缺少 `AmazonEBSCSIDriverPolicy`）
  - `aws-ebs-csi-driver` 未安裝或未就緒
- 解法：
  - 建立/綁定 IAM Role（含 `AmazonEBSCSIDriverPolicy`）到所有節點
    - 建立 trust policy（`trust-policy.json`）：
      - `{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"Service":"ec2.amazonaws.com"},"Action":"sts:AssumeRole"}]}`
    - 建立角色：
      - `aws iam create-role --role-name <role-name> --assume-role-policy-document file://trust-policy.json`
    - `aws iam attach-role-policy --role-name <role-name> --policy-arn arn:aws:iam::aws:policy/service-role/AmazonEBSCSIDriverPolicy`
  - 安裝 EBS CSI Driver：
    - `kubectl apply -k "github.com/kubernetes-sigs/aws-ebs-csi-driver/deploy/kubernetes/overlays/stable/?ref=master"`
  - 建立 `gp3` StorageClass（`k8s/gp3-storageclass.yaml`）
  - 檢查 driver 是否就緒：
    - `kubectl get pods -n kube-system | grep ebs`
