variable "resource_group_name" {
  description = "Existing resource group that holds the application resources."
  type        = string
}

variable "web_app_name" {
  description = "Globally unique name of the Web App."
  type        = string
}

variable "app_service_sku" {
  description = "App Service plan SKU. F1 is the free tier."
  type        = string
  default     = "F1"

  validation {
    condition     = contains(["F1", "B1"], var.app_service_sku)
    error_message = "app_service_sku must be F1 or B1."
  }
}
