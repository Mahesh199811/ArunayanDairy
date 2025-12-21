# AWS Deployment Guide for ArunayanDairy API

Complete step-by-step guide to deploy your .NET API and database to AWS.

## 📋 Table of Contents
1. [Prerequisites](#prerequisites)
2. [AWS Account Setup](#aws-account-setup)
3. [Database Setup (RDS)](#database-setup-rds)
4. [API Deployment Options](#api-deployment-options)
5. [Security Configuration](#security-configuration)
6. [Update MAUI App](#update-maui-app)
7. [Cost Estimation](#cost-estimation)

---

## Prerequisites

### What You Need:
- ✅ AWS Account (Free tier available)
- ✅ Credit/Debit card (required for AWS signup)
- ✅ Your .NET API project (already have it)
- ✅ Basic terminal/command line knowledge

### Tools to Install:
```bash
# AWS CLI
brew install awscli

# Configure AWS CLI (after creating AWS account)
aws configure
```

---

## AWS Account Setup

### Step 1: Create AWS Account
1. Go to: https://aws.amazon.com/
2. Click "Create an AWS Account"
3. Fill in:
   - Email address
   - Password
   - AWS account name
4. Choose "Personal" account type
5. Enter payment information (won't be charged if staying in free tier)
6. Verify phone number
7. Select "Basic Support - Free"

### Step 2: Sign in to AWS Console
1. Go to: https://console.aws.amazon.com/
2. Sign in with your credentials
3. Select your preferred region (e.g., `us-east-1` or closest to your location)

---

## Database Setup (RDS)

### Option 1: Amazon RDS SQL Server (Easier, but costs more)

#### Step 1: Create RDS SQL Server Instance

1. **Navigate to RDS:**
   - In AWS Console, search for "RDS"
   - Click "Create database"

2. **Configuration:**
   ```
   Engine: Microsoft SQL Server
   Edition: SQL Server Express Edition (Free tier eligible)
   Template: Free tier
   
   DB Instance Identifier: arunayan-dairy-db
   Master Username: admin
   Master Password: [Create strong password - SAVE THIS!]
   
   DB Instance Class: db.t3.micro (Free tier)
   Storage: 20 GB (Free tier)
   
   VPC: Default VPC
   Public Access: Yes (for development)
   VPC Security Group: Create new → arunayan-sg
   
   Initial Database Name: ArunayanDairy
   ```

3. **Click "Create database"** (Takes 5-10 minutes)

4. **Get Connection Details:**
   - Click on your database instance
   - Copy the "Endpoint" (e.g., `arunayan-dairy-db.xxxxxx.us-east-1.rds.amazonaws.com`)
   - Port: `1433`

5. **Update Security Group:**
   - Go to EC2 → Security Groups
   - Find `arunayan-sg`
   - Edit Inbound Rules
   - Add Rule:
     - Type: MSSQL
     - Port: 1433
     - Source: `0.0.0.0/0` (for development) or Your IP

#### Connection String:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=arunayan-dairy-db.xxxxxx.us-east-1.rds.amazonaws.com,1433;Database=ArunayanDairy;User Id=admin;Password=YourPassword;TrustServerCertificate=True;"
}
```

**Cost:** ~$15-20/month after free tier expires

---

### Option 2: Amazon RDS PostgreSQL (Recommended - Cheaper & Free tier)

#### Step 1: Create RDS PostgreSQL Instance

1. **Navigate to RDS:**
   - In AWS Console, search for "RDS"
   - Click "Create database"

2. **Configuration:**
   ```
   Engine: PostgreSQL
   Version: PostgreSQL 15.x (latest)
   Template: Free tier
   
   DB Instance Identifier: arunayan-dairy-db
   Master Username: postgres
   Master Password: Bethone1998
   
   DB Instance Class: db.t3.micro (Free tier)
   Storage: 20 GB (Free tier)
   
   VPC: Default VPC
   Public Access: Yes (for development)
   VPC Security Group: Create new → arunayan-sg
   
   Initial Database Name: arunayandb
   ```

3. **Click "Create database"** (Takes 5-10 minutes)

4. **Get Connection Details:**
   - Copy the "Endpoint"
   - Port: `5432`

5. **Update Security Group:**
   - Go to EC2 → Security Groups
   - Find `arunayan-sg`
   - Edit Inbound Rules
   - Add Rule:
     - Type: PostgreSQL
     - Port: 5432
     - Source: `0.0.0.0/0` (for development)

#### Update Your .NET Project:

**Install PostgreSQL package:**
```bash
cd ArunayanDairy.API
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

**Update `Program.cs`:**
```csharp
// Replace UseSqlite with UseNpgsql
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Update `appsettings.json`:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=arunayan-dairy-db.xxxxxx.us-east-1.rds.amazonaws.com;Port=5432;Database=arunayandb;Username=postgres;Password=YourPassword;SSL Mode=Require;"
}
```

**Cost:** FREE for 12 months (750 hours/month), then ~$15/month

---

## API Deployment Options

### Option A: AWS Elastic Beanstalk (Easiest - Recommended for Beginners)

#### Step 1: Prepare Your Application

```bash
cd /Users/maheshgadhave/Downloads/ArunayanDairy/ArunayanDairy.API

# Publish the application
dotnet publish -c Release -o ./publish
```

#### Step 2: Create Deployment Package

```bash
cd publish
zip -r ../arunayan-api.zip .
cd ..
```

#### Step 3: Deploy to Elastic Beanstalk

1. **Navigate to Elastic Beanstalk:**
   - In AWS Console, search for "Elastic Beanstalk"
   - Click "Create application"

2. **Configuration:**
   ```
   Application name: ArunayanDairyAPI
   Platform: .NET Core on Linux
   Platform branch: .NET 8 running on 64bit Amazon Linux 2023
   Platform version: (Latest)
   
   Application code: Upload your code
   Choose file: Select arunayan-api.zip
   ```

3. **Configure more options:**
   - Click "Configure more options"
   - Software → Edit:
     - Environment properties:
       - `ConnectionStrings__DefaultConnection`: [Your RDS connection string]
       - `Jwt__Secret`: [Your JWT secret]
       - `Jwt__Issuer`: ArunayanDairyAPI
       - `Jwt__Audience`: ArunayanDairyMAUI
       - `ASPNETCORE_ENVIRONMENT`: Production

4. **Click "Create application"** (Takes 5-10 minutes)

5. **Get Your API URL:**
   - After deployment completes
   - You'll see: `http://arunayanairyapi-env.xxxxxx.us-east-1.elasticbeanstalk.com`

**Cost:** FREE for 12 months (750 hours/month), then ~$20-30/month

---

### Option B: AWS EC2 (More Control, Manual Setup)

#### Step 1: Launch EC2 Instance

1. **Navigate to EC2:**
   - In AWS Console, search for "EC2"
   - Click "Launch Instance"

2. **Configuration:**
   ```
   Name: ArunayanDairyAPI
   
   Application and OS Images:
   - Ubuntu Server 22.04 LTS (Free tier eligible)
   
   Instance type: t2.micro (Free tier eligible)
   
   Key pair: Create new key pair
     - Name: arunayan-key
     - Type: RSA
     - Format: .pem (for Mac)
     - Download and save securely
   
   Network settings:
   - Allow SSH (port 22) from Your IP
   - Allow HTTP (port 80) from Anywhere
   - Allow HTTPS (port 443) from Anywhere
   - Add rule: Custom TCP, port 5001, from Anywhere
   
   Storage: 8 GB (Free tier)
   ```

3. **Click "Launch instance"**

#### Step 2: Connect to EC2 Instance

```bash
# Change key permissions
chmod 400 ~/Downloads/arunayan-key.pem

# Get public IP from EC2 dashboard (e.g., 54.123.45.67)
ssh -i ~/Downloads/arunayan-key.pem ubuntu@54.123.45.67
```

#### Step 3: Install .NET on EC2

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 9
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verify installation
dotnet --version
```

#### Step 4: Deploy Your Application

**On your Mac:**
```bash
cd /Users/maheshgadhave/Downloads/ArunayanDairy/ArunayanDairy.API

# Publish application
dotnet publish -c Release -o ./publish

# Transfer to EC2
scp -i ~/Downloads/arunayan-key.pem -r ./publish/* ubuntu@54.123.45.67:~/ArunayanDairy/
```

**On EC2 instance:**
```bash
cd ~/ArunayanDairy

# Update appsettings.json with RDS connection string
nano appsettings.json
# Update ConnectionStrings

# Run migrations
dotnet ef database update

# Run the application
dotnet ArunayanDairy.API.dll --urls "http://0.0.0.0:5001"
```

#### Step 5: Set Up as a Service (Keep Running)

```bash
sudo nano /etc/systemd/system/arunayan-api.service
```

Add:
```ini
[Unit]
Description=Arunayan Dairy API

[Service]
WorkingDirectory=/home/ubuntu/ArunayanDairy
ExecStart=/home/ubuntu/.dotnet/dotnet /home/ubuntu/ArunayanDairy/ArunayanDairy.API.dll --urls "http://0.0.0.0:5001"
Restart=always
RestartSec=10
SyslogIdentifier=arunayan-api
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable arunayan-api
sudo systemctl start arunayan-api
sudo systemctl status arunayan-api
```

**Your API URL:** `http://54.123.45.67:5001`

**Cost:** FREE for 12 months (750 hours/month), then ~$10/month

---

### Option C: AWS App Runner (Simplest Container Deployment)

#### Step 1: Create Dockerfile

Create `Dockerfile` in `ArunayanDairy.API/`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ArunayanDairy.API.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArunayanDairy.API.dll"]
```

#### Step 2: Push to ECR (Elastic Container Registry)

```bash
# Create ECR repository
aws ecr create-repository --repository-name arunayan-dairy-api

# Get login token
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [YOUR_ACCOUNT_ID].dkr.ecr.us-east-1.amazonaws.com

# Build and push
cd ArunayanDairy.API
docker build -t arunayan-dairy-api .
docker tag arunayan-dairy-api:latest [YOUR_ACCOUNT_ID].dkr.ecr.us-east-1.amazonaws.com/arunayan-dairy-api:latest
docker push [YOUR_ACCOUNT_ID].dkr.ecr.us-east-1.amazonaws.com/arunayan-dairy-api:latest
```

#### Step 3: Deploy to App Runner

1. Go to AWS App Runner
2. Click "Create service"
3. Select "Container registry"
4. Browse to your ECR image
5. Configure:
   - Port: 8080
   - Environment variables: Add your connection strings
6. Create service

**Cost:** ~$5-10/month (pay per use)

---

## Security Configuration

### 1. Use AWS Secrets Manager

Store sensitive data securely:

```bash
# Create secret
aws secretsmanager create-secret \
  --name ArunayanDairy/ConnectionString \
  --secret-string "Your-RDS-Connection-String"

aws secretsmanager create-secret \
  --name ArunayanDairy/JwtSecret \
  --secret-string "Your-JWT-Secret"
```

### 2. Update Security Groups

**For Production:**
- RDS: Only allow connections from your API's security group
- API: Only allow HTTP/HTTPS from internet

### 3. Enable HTTPS

**Option 1: Use AWS Certificate Manager + Load Balancer**
- Request free SSL certificate
- Attach to Application Load Balancer
- Your API will be: `https://api.yourdomainname.com`

**Option 2: Use Elastic Beanstalk with HTTPS**
- Elastic Beanstalk can handle SSL automatically

---

## Update MAUI App

### Update API Base URL

**File:** `ArunayanDairy/Services/AuthService.cs`

```csharp
private readonly HttpClient _httpClient;

public AuthService()
{
    _httpClient = new HttpClient
    {
        // Replace with your AWS URL
        BaseAddress = new Uri("http://arunayanairyapi-env.xxxxxx.elasticbeanstalk.com")
        // OR for EC2:
        // BaseAddress = new Uri("http://54.123.45.67:5001")
        // OR for production with domain:
        // BaseAddress = new Uri("https://api.arunayandairy.com")
    };
}
```

### For iOS Simulator (Special Case)

iOS simulator can't access `localhost` or `127.0.0.1`. Use:
```csharp
#if DEBUG
    #if IOS
        BaseAddress = new Uri("http://YOUR-MAC-IP:5001")  // e.g., http://192.168.1.50:5001
    #else
        BaseAddress = new Uri("http://localhost:5001")
    #endif
#else
    BaseAddress = new Uri("https://api.arunayandairy.com")  // Production
#endif
```

---

## Cost Estimation

### AWS Free Tier (First 12 Months):
- ✅ EC2 t2.micro: 750 hours/month (FREE)
- ✅ RDS t3.micro: 750 hours/month (FREE)
- ✅ 20 GB database storage (FREE)
- ✅ 5 GB S3 storage (FREE)
- ✅ Data transfer: 15 GB/month (FREE)

### After Free Tier:

**Option 1: Budget Setup (~$30/month)**
- EC2 t2.micro: $8/month
- RDS PostgreSQL t3.micro: $15/month
- Data transfer: $5/month

**Option 2: Elastic Beanstalk (~$40/month)**
- Elastic Beanstalk: $25/month
- RDS PostgreSQL: $15/month

**Option 3: Serverless (~$10-20/month)**
- App Runner: $5-10/month
- RDS Serverless: $5-10/month

---

## Step-by-Step Deployment Checklist

### Phase 1: Database Setup (30 minutes)
- [ ] Create AWS account
- [ ] Create RDS PostgreSQL instance
- [ ] Update security group to allow connections
- [ ] Test connection from your Mac
- [ ] Update local appsettings.json with RDS connection
- [ ] Run migrations: `dotnet ef database update`

### Phase 2: API Deployment (45 minutes)
- [ ] Choose deployment method (Elastic Beanstalk recommended)
- [ ] Publish application
- [ ] Deploy to AWS
- [ ] Configure environment variables
- [ ] Test API endpoints (use Swagger)

### Phase 3: MAUI App Update (15 minutes)
- [ ] Update API base URL in AuthService
- [ ] Test login/signup from MAUI app
- [ ] Verify data is being stored in RDS

### Phase 4: Production Readiness (Optional)
- [ ] Set up custom domain
- [ ] Enable HTTPS
- [ ] Configure monitoring/logging
- [ ] Set up automated backups

---

## Testing Your Deployment

### 1. Test Database Connection
```bash
# From your Mac (after installing PostgreSQL client)
brew install postgresql
psql -h arunayan-dairy-db.xxxxxx.us-east-1.rds.amazonaws.com -U postgres -d arunayandb
```

### 2. Test API
```bash
# Replace with your actual API URL
curl http://your-api-url.elasticbeanstalk.com/swagger/index.html

# Test signup
curl -X POST http://your-api-url/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","password":"Test123!"}'
```

### 3. Test from MAUI App
- Update API URL
- Run app on iOS simulator
- Try signup/login
- Check AWS RDS to verify data

---

## Troubleshooting

### Common Issues:

**1. Can't connect to RDS:**
- Check security group allows your IP
- Verify endpoint and port
- Check VPC settings (must be publicly accessible for development)

**2. API not starting on EC2:**
- Check logs: `sudo journalctl -u arunayan-api -f`
- Verify .NET is installed: `dotnet --version`
- Check firewall: `sudo ufw status`

**3. MAUI app can't connect:**
- Verify API URL is correct
- Check API is running: `curl http://your-api-url/swagger`
- For iOS, make sure using IP address, not localhost

**4. Database migration errors:**
- Ensure connection string is correct
- Check RDS instance is running
- Verify security group allows connections

---

## Next Steps

After successful deployment:

1. **Monitor Your Resources:**
   - AWS CloudWatch for logs
   - RDS Performance Insights
   - Set up billing alerts

2. **Optimize:**
   - Enable RDS automatic backups
   - Set up CloudFront CDN (if needed)
   - Implement caching (Redis)

3. **Scale:**
   - Add auto-scaling groups
   - Use Load Balancer
   - Move to containerized deployment (ECS/EKS)

---

## Recommended Deployment Path for Beginners

**Best approach:**

1. **Start with Elastic Beanstalk + RDS PostgreSQL**
   - Easiest to set up
   - Free tier available
   - Automatic scaling
   - Built-in monitoring

2. **Steps:**
   ```
   Day 1: Set up RDS PostgreSQL (1 hour)
   Day 1: Deploy API to Elastic Beanstalk (1 hour)
   Day 2: Test and update MAUI app (1 hour)
   Day 3: Add custom domain + HTTPS (optional)
   ```

3. **Total Cost:**
   - First 12 months: FREE (within free tier limits)
   - After: ~$40/month

---

## Support Resources

- AWS Free Tier: https://aws.amazon.com/free/
- AWS Documentation: https://docs.aws.amazon.com/
- .NET on AWS: https://aws.amazon.com/developer/language/net/
- AWS Support Forums: https://repost.aws/

---

**Need Help?** Follow this guide step-by-step. Start with database setup first, then move to API deployment.
