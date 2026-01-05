# CI/CD Pipeline Setup

This repository uses GitHub Actions to automatically deploy the API to AWS Elastic Beanstalk when changes are pushed to the main branch.

## Required GitHub Secrets

You need to add these secrets to your GitHub repository:

### 1. Navigate to Repository Settings
Go to: `Settings` → `Secrets and variables` → `Actions` → `New repository secret`

### 2. Add These Secrets:

#### AWS_ACCESS_KEY_ID
Your AWS IAM access key ID with permissions for:
- Elastic Beanstalk (full access or specific environment access)
- S3 (read/write to EB bucket)

#### AWS_SECRET_ACCESS_KEY
Your AWS IAM secret access key (pair of above access key)

#### AWS_ACCOUNT_ID
Your AWS account ID (12-digit number)

## How to Get AWS Credentials

### Option 1: Create IAM User (Recommended)

1. Go to AWS Console → IAM → Users
2. Click "Create user"
3. User name: `github-actions-deployer`
4. Click "Next"
5. Select "Attach policies directly"
6. Add these policies:
   - `AWSElasticBeanstalkFullAccess`
   - `AmazonS3FullAccess` (or create custom policy for EB bucket only)
7. Click "Next" → "Create user"
8. Go to user → Security credentials → Create access key
9. Select "Application running outside AWS" → Create
10. Copy the Access Key ID and Secret Access Key

### Option 2: Get Account ID

```bash
aws sts get-caller-identity --query Account --output text
```

Or find it in AWS Console → Account dropdown (top right)

## Pipeline Workflow

When you push to `main` branch:

1. ✅ Checks out code
2. ✅ Sets up .NET 8
3. ✅ Restores dependencies
4. ✅ Builds the project
5. ✅ Publishes as self-contained for Linux
6. ✅ Creates Procfile
7. ✅ Zips deployment package
8. ✅ Uploads to S3
9. ✅ Creates EB application version
10. ✅ Updates EB environment
11. ✅ Waits for deployment to complete

## Triggering Deployment

Simply commit and push changes:

```bash
git add .
git commit -m "feat: your feature description"
git push origin main
```

The pipeline will automatically:
- Build your code
- Run tests (if configured)
- Deploy to AWS Elastic Beanstalk

## Monitoring Deployment

1. **GitHub**: Go to "Actions" tab to see pipeline status
2. **AWS Console**: Check Elastic Beanstalk environment health
3. **API**: Test at `http://arunayandairy-dev.eba-bzatme36.eu-west-1.elasticbeanstalk.com`

## Pipeline Configuration

The workflow file is located at: `.github/workflows/deploy.yml`

### Customization Options:

#### Deploy on Different Branch
```yaml
on:
  push:
    branches:
      - develop  # Change to your branch
```

#### Add Manual Trigger
```yaml
on:
  push:
    branches:
      - main
  workflow_dispatch:  # Allows manual trigger from GitHub UI
```

#### Deploy to Multiple Environments
```yaml
jobs:
  deploy-dev:
    # ... existing steps
    
  deploy-prod:
    needs: deploy-dev
    if: github.ref == 'refs/heads/main'
    # ... steps for production
```

## Troubleshooting

### Pipeline Fails at AWS Step
- Check if AWS credentials are correct
- Verify IAM user has required permissions
- Ensure AWS_ACCOUNT_ID is correct

### Deployment Succeeds but API Not Working
- Check EB logs: `eb logs` or AWS Console
- Verify environment variables are set
- Check application health in EB dashboard

### Build Fails
- Verify .NET SDK version matches project
- Check for missing dependencies
- Review build logs in GitHub Actions

## Security Best Practices

1. ✅ Use IAM user with minimal required permissions
2. ✅ Rotate access keys regularly
3. ✅ Never commit AWS credentials to repository
4. ✅ Use GitHub secrets for sensitive data
5. ✅ Enable branch protection rules
6. ✅ Require pull request reviews before merging

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [AWS Elastic Beanstalk CLI](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/eb-cli3.html)
- [IAM Best Practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html)
