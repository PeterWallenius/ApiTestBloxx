data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

locals {
  suffix  = substr(md5(data.azurerm_resource_group.main.id), 0, 6)
  is_free = var.app_service_sku == "F1"

  # Built-in role definition IDs (the pipeline's RBAC condition only allows these two).
  role_storage_table_data_contributor = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/providers/Microsoft.Authorization/roleDefinitions/0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3"
  role_key_vault_secrets_user         = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/providers/Microsoft.Authorization/roleDefinitions/4633458b-17de-408a-b874-0445c86b69e6"
}

# --- Storage (Table Storage for call history and saved requests) ---

resource "azurerm_storage_account" "main" {
  name                = "stapitestbloxx${local.suffix}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location

  account_kind             = "StorageV2"
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  shared_access_key_enabled       = false
  default_to_oauth_authentication = true
  allow_nested_items_to_be_public = false
}

resource "azurerm_storage_table" "tables" {
  for_each = toset(["ApiCallHistory", "SavedRequests"])

  name               = each.key
  storage_account_id = azurerm_storage_account.main.id
}

# --- Key Vault (secrets such as BloxxApi--IdToken) ---

resource "azurerm_key_vault" "main" {
  name                = "kv-apitestbloxx-${local.suffix}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  rbac_authorization_enabled = true
  soft_delete_retention_days = 7
  purge_protection_enabled   = false
}

# --- App Service ---

resource "azurerm_service_plan" "main" {
  name                = "asp-apitestbloxx"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku
}

resource "azurerm_linux_web_app" "main" {
  name                = var.web_app_name
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  https_only = true
  # Blazor Server circuits must stay on the same instance.
  client_affinity_enabled                        = true
  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    "KeyVault__Uri"          = azurerm_key_vault.main.vault_uri
    "Storage__TableEndpoint" = azurerm_storage_account.main.primary_table_endpoint
  }

  site_config {
    always_on           = !local.is_free
    websockets_enabled  = true
    http2_enabled       = true
    minimum_tls_version = "1.2"
    ftps_state          = "Disabled"

    application_stack {
      dotnet_version = "10.0"
    }
  }

  logs {
    application_logs {
      file_system_level = "Information"
    }

    http_logs {
      file_system {
        retention_in_days = 3
        retention_in_mb   = 35
      }
    }
  }
}

# --- Managed identity access ---

resource "azurerm_role_assignment" "web_app_table_data" {
  scope              = azurerm_storage_account.main.id
  role_definition_id = local.role_storage_table_data_contributor
  principal_id       = azurerm_linux_web_app.main.identity[0].principal_id
  principal_type     = "ServicePrincipal"
}

resource "azurerm_role_assignment" "web_app_key_vault_secrets" {
  scope              = azurerm_key_vault.main.id
  role_definition_id = local.role_key_vault_secrets_user
  principal_id       = azurerm_linux_web_app.main.identity[0].principal_id
  principal_type     = "ServicePrincipal"
}
