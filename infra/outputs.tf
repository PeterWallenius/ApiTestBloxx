output "web_app_url" {
  value = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "key_vault_name" {
  value = azurerm_key_vault.main.name
}

output "storage_account_name" {
  value = azurerm_storage_account.main.name
}
