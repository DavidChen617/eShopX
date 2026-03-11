locals {
  tags = {
    Project = var.project_name
  }
  
  ssh_allowed_cidr = var.ssh_allowed_cidr != ""? var.ssh_allowed_cidr : "${chomp(data.http.myip.response_body)}/32"
}