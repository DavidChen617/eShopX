resource "tls_private_key" "ssh" {
  algorithm = "ED25519"
}

resource "aws_key_pair" "k8s" {
  key_name   = "${var.project_name}-k8s"
  public_key = tls_private_key.ssh.public_key_openssh
}

resource "random_string" "token_id" {
  length  = 6
  lower   = true
  upper   = false
  number  = true
  special = false
}

resource "random_string" "token_secret" {
  length  = 16
  lower   = true
  upper   = false
  number  = true
  special = false
}

locals {
  kubeadm_token = "${random_string.token_id.result}.${random_string.token_secret.result}"
}

resource "aws_instance" "master" {
  ami                         = data.aws_ami.ubuntu.id
  instance_type               = var.instance_type
  subnet_id                   = aws_subnet.public.id
  vpc_security_group_ids      = [aws_security_group.master.id]
  associate_public_ip_address = true
  key_name                    = aws_key_pair.k8s.key_name

  user_data = <<-EOF
    #!/usr/bin/env bash
    set -euo pipefail

    swapoff -a
    sed -i '/ swap / s/^/#/' /etc/fstab

    cat <<SYSCTL | tee /etc/sysctl.d/99-k8s.conf
    net.bridge.bridge-nf-call-iptables  = 1
    net.ipv4.ip_forward                 = 1
    net.bridge.bridge-nf-call-ip6tables = 1
    SYSCTL
    sysctl --system

    apt-get update
    apt-get install -y ca-certificates curl gnupg lsb-release

    mkdir -p /etc/apt/keyrings
    curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.34/deb/Release.key | gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg
    echo "deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.34/deb/ /" | tee /etc/apt/sources.list.d/kubernetes.list

    apt-get update
    apt-get install -y containerd kubelet kubeadm kubectl
    apt-mark hold kubelet kubeadm kubectl

    mkdir -p /etc/containerd
    containerd config default | tee /etc/containerd/config.toml
    systemctl restart containerd
    systemctl enable containerd

    systemctl enable kubelet

    PRIVATE_IP=$(curl -s http://169.254.169.254/latest/meta-data/local-ipv4)

    kubeadm init \
      --apiserver-advertise-address=${PRIVATE_IP} \
      --pod-network-cidr=192.168.0.0/16 \
      --token ${local.kubeadm_token} \
      --token-ttl 0

    mkdir -p /home/ubuntu/.kube
    cp /etc/kubernetes/admin.conf /home/ubuntu/.kube/config
    chown ubuntu:ubuntu /home/ubuntu/.kube/config

    # Calico
    su - ubuntu -c "kubectl apply -f https://raw.githubusercontent.com/projectcalico/calico/v3.27.3/manifests/calico.yaml"

    echo "kubeadm join ${PRIVATE_IP}:6443 --token ${local.kubeadm_token} --discovery-token-unsafe-skip-ca-verification" > /home/ubuntu/join.sh
    chmod +x /home/ubuntu/join.sh
  EOF

  tags = merge(local.tags, { Name = "${var.project_name}-master" })
}

resource "aws_instance" "workers" {
  count                  = 2
  ami                    = data.aws_ami.ubuntu.id
  instance_type          = var.instance_type
  subnet_id              = aws_subnet.private.id
  vpc_security_group_ids = [aws_security_group.worker.id]
  key_name               = aws_key_pair.k8s.key_name

  user_data = <<-EOF
    #!/usr/bin/env bash
    set -euo pipefail

    swapoff -a
    sed -i '/ swap / s/^/#/' /etc/fstab

    cat <<SYSCTL | tee /etc/sysctl.d/99-k8s.conf
    net.bridge.bridge-nf-call-iptables  = 1
    net.ipv4.ip_forward                 = 1
    net.bridge.bridge-nf-call-ip6tables = 1
    SYSCTL
    sysctl --system

    apt-get update
    apt-get install -y ca-certificates curl gnupg lsb-release

    mkdir -p /etc/apt/keyrings
    curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.34/deb/Release.key | gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg
    echo "deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.34/deb/ /" | tee /etc/apt/sources.list.d/kubernetes.list

    apt-get update
    apt-get install -y containerd kubelet kubeadm kubectl
    apt-mark hold kubelet kubeadm kubectl

    mkdir -p /etc/containerd
    containerd config default | tee /etc/containerd/config.toml
    systemctl restart containerd
    systemctl enable containerd

    systemctl enable kubelet

    # wait for master to be ready, then join
    sleep 60
    kubeadm join ${aws_instance.master.private_ip}:6443 --token ${local.kubeadm_token} --discovery-token-unsafe-skip-ca-verification
  EOF

  tags = merge(local.tags, { Name = "${var.project_name}-worker-${count.index + 1}" })
}
