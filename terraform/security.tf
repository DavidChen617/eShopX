resource "aws_security_group" "master" {
  name        = "${var.project_name}-master"
  description = "K8s control-plane"
  vpc_id      = aws_vpc.main.id
  tags        = merge(local.tags, { Name = "${var.project_name}-master" })
}

resource "aws_security_group" "worker" {
  name        = "${var.project_name}-worker"
  description = "K8s workers"
  vpc_id      = aws_vpc.main.id
  tags        = merge(local.tags, { Name = "${var.project_name}-worker" })
}

# SSH to master
resource "aws_security_group_rule" "master_ssh" {
  type              = "ingress"
  from_port         = 22
  to_port           = 22
  protocol          = "tcp"
  cidr_blocks       = [var.ssh_allowed_cidr]
  security_group_id = aws_security_group.master.id
}

# HTTP/HTTPS to master (reverse proxy)
resource "aws_security_group_rule" "master_http" {
  type              = "ingress"
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.master.id
}

resource "aws_security_group_rule" "master_https" {
  type              = "ingress"
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.master.id
}

# Allow workers to reach control-plane
resource "aws_security_group_rule" "master_from_worker_all" {
  type                     = "ingress"
  from_port                = 0
  to_port                  = 0
  protocol                 = "-1"
  source_security_group_id = aws_security_group.worker.id
  security_group_id        = aws_security_group.master.id
}

# Worker <-> worker, master -> worker
resource "aws_security_group_rule" "worker_from_master_all" {
  type                     = "ingress"
  from_port                = 0
  to_port                  = 0
  protocol                 = "-1"
  source_security_group_id = aws_security_group.master.id
  security_group_id        = aws_security_group.worker.id
}

resource "aws_security_group_rule" "worker_from_worker_all" {
  type                     = "ingress"
  from_port                = 0
  to_port                  = 0
  protocol                 = "-1"
  source_security_group_id = aws_security_group.worker.id
  security_group_id        = aws_security_group.worker.id
}

# Egress all
resource "aws_security_group_rule" "master_egress" {
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.master.id
}

resource "aws_security_group_rule" "worker_egress" {
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.worker.id
}
