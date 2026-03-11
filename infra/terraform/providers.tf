terraform {
  // terraform 版本
  required_version = ">= 1.6.0"
  // 宣告本專案會用到的 provider 與版本範圍
  required_providers {
    // 建立 aws 資源 (ec2, vpc, igw 等)
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    // 產生 SSH 金鑰
    tls = {
      source  = "hashicorp/tls"
      version = "~> 4.0"
    }
    // 產生隨機字串（例如資源命名）
    random = {
      source  = "hashicorp/random"
      version = "3.7.2"
    }
    // 寫本機檔案（把私鑰存到本機）
    local = {
      source  = "hashicorp/local"
      version = "~> 2.5"
    }
    // 讀外部網址（拿目前的公網 IP 來做 SSH 白名單）
    http = {
      source  = "hashicorp/http"
      version = "~> 3.5"
    }
  }
}
// 指定 AWS 區域
provider "aws" {
  region = var.region
}

