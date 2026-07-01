# Deployment & Secrets Management Guide

## Overview

This guide covers secure deployment of Naar-Noor with proper secret management for production environments.

---

## ⚠️ NEVER DO THIS

- ❌ Commit `.env` files to git
- ❌ Hardcode secrets in docker-compose.yml, Dockerfile, or config files
- ❌ Pass secrets as command-line arguments (visible in process lists)
- ❌ Store secrets in environment variables on disk unencrypted
- ❌ Use the same secrets across dev/staging/production

---

## ✅ DO THIS INSTEAD

### Option 1: Environment Variables (Development Only)

For local development **only**:

```bash
# Set environment variables in your shell
export SUPABASE_DB_HOST="db.your-project.supabase.co"
export SUPABASE_DB_PASSWORD="your-actual-password"
export SUPABASE_URL="https://your-project.supabase.co"
export SUPABASE_ANON_KEY="your-anon-key"
export SUPABASE_SERVICE_ROLE_KEY="your-service-role-key"

# Run docker-compose
docker-compose up
```

**Verify secrets are NOT exposed**:

```bash
# ❌ These would show secrets (DON'T DO THIS)
docker ps -e  # Wrong
env | grep SUPABASE  # Shows values in process list

# ✅ This is safe
docker-compose exec naar-noor-prod-api env | grep SUPABASE  # Inside container only
```

---

### Option 2: Docker Secrets (Docker Swarm / Kubernetes)

#### For Docker Swarm:

```bash
# Create secrets
echo -n "your-db-password" | docker secret create supabase_db_password -
echo -n "your-anon-key" | docker secret create supabase_anon_key -
echo -n "your-service-role-key" | docker secret create supabase_service_role_key -

# Use secrets in compose file
services:
  naar-noor-prod-api:
    environment:
      SUPABASE_DB_PASSWORD_FILE: /run/secrets/supabase_db_password
      SUPABASE_ANON_KEY_FILE: /run/secrets/supabase_anon_key
```

#### For Kubernetes:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: supabase-secrets
type: Opaque
stringData:
  db-password: "your-db-password"
  anon-key: "your-anon-key"
  service-role-key: "your-service-role-key"
---
apiVersion: v1
kind: Pod
metadata:
  name: naar-noor-api
spec:
  containers:
  - name: api
    image: naar-noor:backend
    env:
    - name: SUPABASE_DB_PASSWORD
      valueFrom:
        secretKeyRef:
          name: supabase-secrets
          key: db-password
    - name: SUPABASE_ANON_KEY
      valueFrom:
        secretKeyRef:
          name: supabase-secrets
          key: anon-key
```

---

### Option 3: Cloud Secret Manager

#### AWS Secrets Manager:

```bash
# Create secret
aws secretsmanager create-secret \
  --name naar-noor/production \
  --secret-string '{
    "SUPABASE_DB_PASSWORD": "your-db-password",
    "SUPABASE_ANON_KEY": "your-anon-key",
    "SUPABASE_SERVICE_ROLE_KEY": "your-service-role-key"
  }'

# In Dockerfile or ECS task:
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apk add --no-cache aws-cli
COPY entrypoint.sh .
ENTRYPOINT ["./entrypoint.sh"]
```

#### Azure Key Vault:

```bash
# Create vault and secrets
az keyvault create --name naar-noor-vault --resource-group mygroup
az keyvault secret set --vault-name naar-noor-vault --name SuperbaseDbPassword --value "your-password"

# Reference in .NET configuration
var keyVaultUrl = "https://naar-noor-vault.vault.azure.net/";
builder.Configuration.AddAzureKeyVault(
  new Uri(keyVaultUrl),
  new DefaultAzureCredential());
```

#### HashiCorp Vault:

```bash
# Write secret
vault kv put secret/naar-noor/production \
  SUPABASE_DB_PASSWORD="your-db-password" \
  SUPABASE_ANON_KEY="your-anon-key"

# In application (via Vault .NET library)
var vaultClient = new VaultClient(new VaultClientSettings(vaultUrl, authMethod));
var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("secret/naar-noor/production");
```

---

### Option 4: CI/CD Platform Secrets

#### GitHub Actions:

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

env:
  REGISTRY: docker.io
  IMAGE_NAME: naar-noor/api

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build and push Docker image
        env:
          # ✅ Secrets are injected, never exposed in logs
          SUPABASE_DB_PASSWORD: ${{ secrets.PROD_SUPABASE_DB_PASSWORD }}
          SUPABASE_ANON_KEY: ${{ secrets.PROD_SUPABASE_ANON_KEY }}
          SUPABASE_SERVICE_ROLE_KEY: ${{ secrets.PROD_SUPABASE_SERVICE_ROLE_KEY }}
        run: |
          docker build -t $IMAGE_NAME .
          docker push $IMAGE_NAME
```

#### GitLab CI:

```yaml
deploy:production:
  stage: deploy
  environment: production
  script:
    - docker build -t naar-noor:api .
    - docker push naar-noor:api
  variables:
    # ✅ Protected variables only exposed to protected branch
    SUPABASE_DB_PASSWORD: $PROD_SUPABASE_DB_PASSWORD
    SUPABASE_ANON_KEY: $PROD_SUPABASE_ANON_KEY
  only:
    - main
```

---

## Secrets Rotation

### Weekly Rotation Strategy

```bash
#!/bin/bash
# rotate-secrets.sh - Rotates secrets weekly

VAULT_URL="https://vault.example.com"
VAULT_TOKEN="$VAULT_TOKEN"

# 1. Generate new secret
NEW_PASSWORD=$(openssl rand -base64 32)

# 2. Update in Supabase
curl -X POST "$SUPABASE_URL/admin/users/reset-password" \
  -H "Authorization: Bearer $SUPABASE_SERVICE_ROLE_KEY" \
  -H "Content-Type: application/json" \
  -d "{\"password\": \"$NEW_PASSWORD\"}"

# 3. Store in secret manager
curl -X POST "$VAULT_URL/v1/secret/data/naar-noor/db-password" \
  -H "X-Vault-Token: $VAULT_TOKEN" \
  -d "{\"data\": {\"value\": \"$NEW_PASSWORD\"}}"

# 4. Trigger deployment (auto-picks up new secret)
kubectl rollout restart deployment/naar-noor-api -n production

# 5. Verify new secret works
kubectl logs -f deployment/naar-noor-api -n production | grep "Database connected"

# 6. Audit log
echo "[$(date)] Secret rotated for db password" >> /var/log/secret-rotation.log
```

---

## Audit & Compliance

### Secrets Access Logging

Ensure all secret access is logged:

```json
{
  "timestamp": "2026-07-15T10:30:00Z",
  "action": "SECRET_ACCESS",
  "secret_name": "supabase_db_password",
  "accessor": "naar-noor-api-container",
  "environment": "production",
  "ip_address": "10.0.1.5",
  "status": "SUCCESS"
}
```

### Compliance Checklist

- [ ] All secrets rotated at least quarterly
- [ ] Access to secrets logged and audited monthly
- [ ] Secrets never committed to git (check with `git log -p` for past commits)
- [ ] Different secrets for dev/staging/production
- [ ] Secrets not exposed in container logs
- [ ] Secret manager encrypted at rest and in transit
- [ ] Only service accounts with minimal permissions can read secrets
- [ ] Multi-factor authentication required for manual secret access

---

## Quick Reference

| Method | Best For | Complexity | Security | Cost |
|--------|----------|-----------|----------|------|
| Environment Variables | Local dev only | Low | Low | Free |
| Docker Secrets | Docker Swarm | Medium | Medium | Free |
| Kubernetes Secrets | Kubernetes clusters | Medium | Medium | Free |
| AWS Secrets Manager | AWS deployments | High | High | $0.40/secret/month |
| Azure Key Vault | Azure deployments | High | High | $0.03/10k ops |
| HashiCorp Vault | Enterprise | High | Very High | $500-5000/year |
| CI/CD Secrets | GitHub/GitLab | Medium | High | Free (included) |

---

## Troubleshooting

### Secret Not Found

```bash
# Check if secret is set
docker-compose exec naar-noor-prod-api printenv | grep SUPABASE

# If empty, verify environment variable is exported
echo $SUPABASE_DB_PASSWORD  # Should print value

# If still empty, check docker-compose variable syntax
docker-compose config | grep SUPABASE
```

### Secret Exposure in Logs

```bash
# Scan for exposed secrets
git log -p | grep -i "password\|secret\|key"

# Scan Docker image
docker scan naar-noor:api

# If found, use git-filter-repo to remove from history:
git-filter-repo --invert-paths --paths .env
```

---

## References

- [OWASP Secret Management](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
- [Docker Secrets Documentation](https://docs.docker.com/engine/swarm/secrets/)
- [Kubernetes Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
- [AWS Secrets Manager Best Practices](https://docs.aws.amazon.com/secretsmanager/latest/userguide/best-practices.html)

