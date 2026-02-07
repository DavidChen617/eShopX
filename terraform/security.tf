resource "aws_security_group" "ec2" {
  name = "${var.project_name}-ec2-sg"
  description = "EC2 security group"
  vpc_id = aws_vpc.main.id
  
  ingress {
    description = "SSH from allowed CIDR"
    from_port = 22
    to_port = 22
    protocol = "tcp"
    cidr_blocks = [local.ssh_allowed_cidr]
  }

  ingress {
    description = "SSH from same SG"
    from_port = 22
    to_port = 22
    protocol = "tcp"
    self = true
  }

  ingress {
    description = "K8s API from same SG"
    from_port = 6443
    to_port = 6443
    protocol = "tcp"
    self = true
  }

  ingress {
    description = "Kubelet from same SG"
    from_port = 10250
    to_port = 10250
    protocol = "tcp"
    self = true
  }

  ingress {
    description = "Calico VXLAN"
    from_port = 4789
    to_port = 4789
    protocol = "udp"
    self = true
  }

  ingress {
    description = "Calico Typha"
    from_port = 5473
    to_port = 5473
    protocol = "tcp"
    self = true
  }
  
  ingress {
    description = "HTTP"
    from_port = 80
    to_port = 80
    protocol = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  ingress {
    description = "HTTPS"
    from_port = 443
    to_port = 443
    protocol = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "Backend"
    from_port = 5177
    to_port = 5177
    protocol = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  egress {
    description = "All outbound"
    from_port = 0
    to_port = 0
    protocol = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  tags = merge(local.tags, {
    Name = "${var.project_name}-ec2-sg"
  })
}
