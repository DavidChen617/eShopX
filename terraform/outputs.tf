output "master_public_ip" {
  description = "Master EC2 public IP"
  value = aws_instance.master.public_ip
}

output "master_public_dns" {
  description = "Master EC2 public DNS"
  value = aws_instance.master.public_dns
}

output "worker_private_ips" {
  description = "Worker EC2 private IPs"
  value = aws_instance.worker[*].private_ip
}

output "ssh_private_key_path" {
  description = "Path to generated SSH private key"
  value = local_file.private_key.filename
}
