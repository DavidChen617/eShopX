resource "aws_security_group" "master" {
  name        = "${var.project_name}-master-sg"
  description = "Master security group"
  vpc_id      = aws_vpc.main.id

  egress {
    description = "All outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(local.tags, {
    Name = "${var.project_name}-master-sg"
  })
}

resource "aws_security_group" "worker" {
  name        = "${var.project_name}-worker-sg"
  description = "Worker security group"
  vpc_id      = aws_vpc.main.id

  egress {
    description = "All outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(local.tags, {
    Name = "${var.project_name}-worker-sg"
  })
}

resource "aws_security_group_rule" "master_ssh_cidr" {
  type              = "ingress"
  description       = "SSH from allowed CIDR"
  from_port         = 22
  to_port           = 22
  protocol          = "tcp"
  cidr_blocks       = [local.ssh_allowed_cidr]
  security_group_id = aws_security_group.master.id
}

resource "aws_security_group_rule" "master_http" {
  type              = "ingress"
  description       = "HTTP"
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.master.id
}

resource "aws_security_group_rule" "master_https" {
  type              = "ingress"
  description       = "HTTPS"
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.master.id
}

resource "aws_security_group_rule" "master_k8s_api_from_workers" {
  type                     = "ingress"
  description              = "K8s API from workers"
  from_port                = 6443
  to_port                  = 6443
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.worker.id
  security_group_id        = aws_security_group.master.id
}

resource "aws_security_group_rule" "master_typha_from_workers" {
  type                     = "ingress"
  description              = "Calico Typha from workers"
  from_port                = 5473
  to_port                  = 5473
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.worker.id
  security_group_id        = aws_security_group.master.id
}

resource "aws_security_group_rule" "master_vxlan_from_workers" {
  type                     = "ingress"
  description              = "Calico VXLAN from workers"
  from_port                = 4789
  to_port                  = 4789
  protocol                 = "udp"
  source_security_group_id = aws_security_group.worker.id
  security_group_id        = aws_security_group.master.id
}

resource "aws_security_group_rule" "worker_ssh_from_master" {
  type                     = "ingress"
  description              = "SSH from master"
  from_port                = 22
  to_port                  = 22
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.master.id
  security_group_id        = aws_security_group.worker.id
}

resource "aws_security_group_rule" "worker_kubelet_from_master" {
  type                     = "ingress"
  description              = "Kubelet from master"
  from_port                = 10250
  to_port                  = 10250
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.master.id
  security_group_id        = aws_security_group.worker.id
}

resource "aws_security_group_rule" "worker_vxlan_from_master" {
  type                     = "ingress"
  description              = "Calico VXLAN from master"
  from_port                = 4789
  to_port                  = 4789
  protocol                 = "udp"
  source_security_group_id = aws_security_group.master.id
  security_group_id        = aws_security_group.worker.id
}

resource "aws_security_group_rule" "worker_vxlan_from_workers" {
  type                     = "ingress"
  description              = "Calico VXLAN from workers"
  from_port                = 4789
  to_port                  = 4789
  protocol                 = "udp"
  source_security_group_id = aws_security_group.worker.id
  security_group_id        = aws_security_group.worker.id
}
