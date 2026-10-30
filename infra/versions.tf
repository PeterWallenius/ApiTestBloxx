terraform {
  required_version = ">= 1.16.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 5.12"
    }
  }

  # Resource group, storage account and container are passed with -backend-config.
  backend "azurerm" {
    key              = "apitestbloxx.tfstate"
    use_azuread_auth = true
  }
}

provider "azurerm" {
  features {}

  storage_use_azuread = true
}
