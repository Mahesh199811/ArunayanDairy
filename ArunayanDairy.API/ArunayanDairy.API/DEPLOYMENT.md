# Arunayan Dairy API - AWS Elastic Beanstalk Deployment Guide

## Overview
This guide documents the deployment of the Arunayan Dairy .NET API to AWS Elastic Beanstalk.

## Architecture
- **Local Development**: SQLite database (`local.db`)
- **Production**: MySQL on AWS RDS
- **Platform**: .NET 8 on Amazon Linux 2023
- **Deployment**: Self-contained deployment for compatibility

## Prerequisites

### Required Tools
- **.NET 8 SDK** - For building the application
- **AWS CLI** - Configured with valid credentials
- **EB CLI** - AWS Elastic Beanstalk Command Line Interface
  ```bash
  pip3 install awsebcli
  ```

### AWS Account
- Valid AWS account with appropriate permissions
- AWS credentials configured (`~/.aws/credentials`)

## Database Configuration

### Environment-Specific Database Setup

The application uses different databases based on the environment:

**Program.cs Configuration:**
```csharp
// Configure Database - SQLite for Development, MySQL for Production
if (builder.Environment.IsDevelopment())
{
    // Local SQLite
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=local.db"));
}
else
{
    // AWS RDS MySQL (Elastic Beanstalk)
    var connectionString =
        $"Server={Environment.GetEnvironmentVariable("RDS_HOSTNAME")};" +
        $"Port={Environment.GetEnvironmentVariable("RDS_PORT")};" +
        $"Database={Environment.GetEnvironmentVariable("RDS_DB_NAME")};" +
        $"User={Environment.GetEnvironmentVariable("RDS_USERNAME")};" +
        $"Password={Environment.GetEnvironmentVariable("RDS_PASSWORD")};";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}
```

### Required NuGet Packages
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

## Application Configuration

### Port Configuration for Elastic Beanstalk

EB expects applications to listen on **port 5000**. Updated `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 5000 for Elastic Beanstalk
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
});
```

### Swagger Configuration

Enabled Swagger in all environments for easier API testing:

```csharp
// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Arunayan Dairy API v1");
    options.RoutePrefix = "swagger"; // Swagger UI at /swagger
});
```

## Deployment Steps

### 1. Create Entity Framework Migrations

```bash
cd /path/to/ArunayanDairy.API
dotnet ef migrations add InitialCreate
```

### 2. Build the Application

```bash
dotnet build
```

### 3. Initialize Elastic Beanstalk (One-time Setup)

```bash
eb init --interactive
```

**Configuration:**
- **Region**: eu-west-1 (EU - Ireland)
- **Application**: ArunayanDairy
- **Platform**: .NET Core on Linux
- **Platform Branch**: .NET 8 running on 64bit Amazon Linux 2023
- **SSH**: Disabled (optional)

This creates `.elasticbeanstalk/config.yml`:

```yaml
branch-defaults:
  default:
    environment: ArunayanDairy-dev
deploy:
  artifact: publish.zip
global:
  application_name: ArunayanDairy
  default_platform: .NET 8 running on 64bit Amazon Linux 2023
  default_region: eu-west-1
```

### 4. Publish the Application (Self-Contained)

**Important**: Use self-contained deployment to include the .NET runtime:

```bash
rm -rf publish
dotnet publish -c Release -o publish --self-contained -r linux-x64
```

### 5. Create Procfile

Create a `Procfile` in the `publish` directory to specify how to run the application:

```bash
echo 'web: ./ArunayanDairy.API' > publish/Procfile
```

### 6. Package the Application

```bash
rm -f publish.zip
cd publish
zip -r ../publish.zip .
cd ..
```

### 7. Deploy to Elastic Beanstalk

```bash
eb deploy
```

**Deployment Output:**
```
Uploading: [##################################################] 100% Done...
2026-01-05 13:15:12    INFO    Environment update is starting.
2026-01-05 13:15:17    INFO    Deploying new version to instance(s).
2026-01-05 13:15:21    INFO    Instance deployment found a self-contained .NET Core application
2026-01-05 13:15:27    INFO    Instance deployment completed successfully.
2026-01-05 13:15:34    INFO    New application version was deployed to running EC2 instances.
2026-01-05 13:15:34    INFO    Environment update completed successfully.
```

### 8. Verify Deployment

```bash
eb status
```

**Expected Output:**
```
Environment details for: ArunayanDairy-dev
  Application name: ArunayanDairy
  Region: eu-west-1
  Platform: .NET 8 running on 64bit Amazon Linux 2023
  CNAME: ArunayanDairy-dev.eba-bzatme36.eu-west-1.elasticbeanstalk.com
  Status: Ready
  Health: Green
```

## Testing the Deployment

### API Endpoints

- **Base URL**: `http://arunayandairy-dev.eba-bzatme36.eu-west-1.elasticbeanstalk.com`
- **Swagger UI**: `http://arunayandairy-dev.eba-bzatme36.eu-west-1.elasticbeanstalk.com/swagger/`
- **Products**: `http://arunayandairy-dev.eba-bzatme36.eu-west-1.elasticbeanstalk.com/api/products`

### Test Commands

```bash
# Test API health
curl http://arunayandairy-dev.eba-bzatme36.eu-west-1.elasticbeanstalk.com/api/products

# Check Swagger JSON
curl http://arunayandairy-dev.eba-bzatme36.eu-west-1.elasticbeanstalk.com/swagger/v1/swagger.json
```

## Environment Configuration

### Set Environment to Production

```bash
eb setenv ASPNETCORE_ENVIRONMENT=Production
```

### Configure RDS MySQL (Production Database)

When creating an RDS instance and attaching it to your EB environment, the following environment variables are automatically set:

- `RDS_HOSTNAME` - Database endpoint
- `RDS_PORT` - Database port (usually 3306)
- `RDS_DB_NAME` - Database name
- `RDS_USERNAME` - Database username
- `RDS_PASSWORD` - Database password

## Useful EB Commands

```bash
# List environments
eb list

# Check environment status
eb status

# View logs
eb logs

# Open application in browser
eb open

# Set environment variables
eb setenv KEY=VALUE

# Terminate environment
eb terminate ArunayanDairy-dev
```

## Troubleshooting

### Issue: 404 Not Found

**Solution**: 
1. Ensure application listens on port 5000
2. Check Swagger is enabled in production
3. Use trailing slash for Swagger: `/swagger/`

### Issue: Runtime dependency errors

**Solution**: Use self-contained deployment:
```bash
dotnet publish --self-contained -r linux-x64
```

### Issue: Database connection fails

**Solution**: 
1. Verify RDS is attached to EB environment
2. Check environment variables are set
3. Ensure security group allows EB instance to access RDS

### Issue: SSL Certificate Errors

**Solution**: EB uses HTTP by default. For HTTPS, configure:
1. Load balancer with SSL certificate
2. Update application URLs to HTTPS

## Deployment Checklist

- [ ] Database migrations created
- [ ] Application builds successfully
- [ ] EB initialized (`.elasticbeanstalk/config.yml` exists)
- [ ] Self-contained publish created
- [ ] `Procfile` created in publish directory
- [ ] Deployment zip created (`publish.zip`)
- [ ] Environment variables configured
- [ ] Application deployed via `eb deploy`
- [ ] Health check shows "Green"
- [ ] API endpoints accessible
- [ ] Swagger UI accessible

## Security Considerations

### Production Recommendations

1. **HTTPS**: Configure SSL/TLS certificate via AWS Certificate Manager
2. **Environment Variables**: Store sensitive data in EB environment variables, not in code
3. **Database**: Use RDS with proper security groups
4. **Authentication**: JWT tokens are already configured
5. **CORS**: Update CORS policy for production domains

### Update CORS for Production

In `Program.cs`, update CORS policy:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## Cost Optimization

- Use **t2.micro** or **t3.micro** instances for development
- Configure **auto-scaling** based on load
- Use **RDS Reserved Instances** for production
- Enable **S3 lifecycle policies** for old deployment versions

## Next Steps

1. **Set up RDS MySQL database**
2. **Attach RDS to EB environment**
3. **Configure custom domain**
4. **Set up SSL certificate**
5. **Configure CI/CD pipeline**
6. **Set up monitoring and alerts**

## Resources

- [AWS Elastic Beanstalk Documentation](https://docs.aws.amazon.com/elasticbeanstalk/)
- [EB CLI Documentation](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/eb-cli3.html)
- [.NET Core on Elastic Beanstalk](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/create-deploy-dotnet-core.html)

---

**Deployed By**: Mahesh Gadhave  
**Date**: January 5, 2026  
**Environment**: ArunayanDairy-dev  
**Region**: eu-west-1
