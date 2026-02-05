output "master_public_ip" {
  value = aws_instance.master.public_ip
}

output "master_private_ip" {
  value = aws_instance.master.private_ip
}

output "kubeadm_token" {
  value     = local.kubeadm_token
  sensitive = true
}

output "ssh_private_key_path" {
  value = local_file.ssh_private_key_pem.filename
}
