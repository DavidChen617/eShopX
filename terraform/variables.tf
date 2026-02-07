variable "region" {
  type = string
  description = "AWS region"
  default = "ap-northeast-1"
}

variable "project_name" {
  type = string
  description = "Project name used for tags and resource naming"
  default = "eshopx"
}

variable "vpc_cidr" {
  type        = string
  description = "VPC CIDR block"
  default     = "10.0.0.0/16"
}

variable "public_subnet_cidr" {
  type        = string
  description = "Public subnect CIDR block"
  default     = "10.0.1.0/24"
}

variable "private_subnet_cidr" {
  type        = string
  description = "Private subnect CIDR block"
  default     = "10.0.2.0/24"
}

variable "instance_type" {
  type        = string
  description = "EC2 instance type"
  default     = "t3.medium"
}

variable "ami_id" {
  type        = string
  description = "AMI ID for EC2 instance"
  default     = "ami-0f65fc8c24ec8d2a1"
}

variable "ssh_allowed_cidr" {
  type        = string
  description = "SSH allowed CIDR. if empty, auto-detech current public IP."
  default     = ""
}
