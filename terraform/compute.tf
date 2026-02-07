resource "tls_private_key" "ssh" {
  algorithm = "RSA"
  rsa_bits = 4096
}

resource "aws_key_pair" "generated" {
  key_name = "${var.project_name}-key"
  public_key = tls_private_key.ssh.public_key_openssh
  
  tags = merge(local.tags,{
    Name = "${var.project_name}-key"
  })
}

resource "local_file" "private_key" {
  content = tls_private_key.ssh.private_key_pem
  filename = "${path.module}/${var.project_name}-key.pem"
  file_permission = "0600"
}

resource "aws_instance" "master" {
  ami = var.ami_id
  instance_type = var.instance_type
  subnet_id = aws_subnet.public.id
  vpc_security_group_ids = [aws_security_group.ec2.id]
  associate_public_ip_address = true
  key_name = aws_key_pair.generated.key_name
  
  tags = merge(local.tags, {
    Name = "${var.project_name}-master"
  })
}

resource "aws_instance" "worker" {
  count = 2
  ami = var.ami_id
  instance_type = var.instance_type
  subnet_id = aws_subnet.public.id
  vpc_security_group_ids = [aws_security_group.ec2.id]
  associate_public_ip_address = true
  key_name = aws_key_pair.generated.key_name
  
  tags = merge(local.tags, {
    Name = "${var.project_name}-worker-${count.index + 1}"
  })
}
