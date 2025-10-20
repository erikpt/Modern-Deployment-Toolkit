# Deployment Guide

This guide covers various deployment scenarios for the Modern Deployment Toolkit.

## Deployment Options

1. **Development**: Local development and testing
2. **Docker**: Containerized deployment
3. **Kubernetes**: Container orchestration
4. **Windows Service**: Windows background service
5. **Linux Systemd**: Linux background service
6. **IIS**: Internet Information Services (Windows)
7. **Azure**: Cloud deployment

## 1. Development Deployment

### Web API

```bash
cd MDT.WebUI
dotnet run
```

Access the API at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

### Console Engine

```bash
cd MDT.Engine
dotnet run -- <path-to-task-sequence-file>
```

## 2. Docker Deployment

### Build Docker Images

```bash
# Build Web UI image
docker build -t mdt-webui:latest --target webui .

# Build Engine image
docker build -t mdt-engine:latest --target engine .
```

### Run with Docker Compose

```bash
docker-compose up -d
```

This starts:
- Web API on ports 5000 (HTTP) and 5001 (HTTPS)
- PostgreSQL database

Stop with:
```bash
docker-compose down
```

### Run Individual Containers

#### Web UI with SQLite
```bash
docker run -d -p 5000:80 \
  -e Database__UseSqlite=true \
  -v $(pwd)/data:/app/data \
  --name mdt-webui \
  mdt-webui:latest
```

#### Web UI with PostgreSQL
```bash
docker run -d -p 5000:80 \
  -e Database__UseSqlite=false \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Database=mdt;Username=mdt;Password=yourpassword" \
  --name mdt-webui \
  mdt-webui:latest
```

#### Console Engine
```bash
docker run -v $(pwd)/sequences:/sequences \
  mdt-engine:latest /sequences/task-sequence.xml
```

## 3. Kubernetes Deployment

### Create Kubernetes Resources

```yaml
# mdt-deployment.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mdt-config
data:
  appsettings.json: |
    {
      "Database": {
        "UseSqlite": false
      },
      "ConnectionStrings": {
        "DefaultConnection": "Host=postgres;Database=mdt;Username=mdt;Password=yourpassword"
      }
    }
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mdt-webui
spec:
  replicas: 3
  selector:
    matchLabels:
      app: mdt-webui
  template:
    metadata:
      labels:
        app: mdt-webui
    spec:
      containers:
      - name: mdt-webui
        image: mdt-webui:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        volumeMounts:
        - name: config
          mountPath: /app/appsettings.json
          subPath: appsettings.json
      volumes:
      - name: config
        configMap:
          name: mdt-config
---
apiVersion: v1
kind: Service
metadata:
  name: mdt-webui
spec:
  selector:
    app: mdt-webui
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
```

Deploy:
```bash
kubectl apply -f mdt-deployment.yaml
```

## 4. Windows Service Deployment

### Create Windows Service

1. Publish the application:
```bash
dotnet publish MDT.WebUI -c Release -o ./publish
```

2. Create Windows Service:
```powershell
sc.exe create MDTWebUI binPath="C:\path\to\publish\MDT.WebUI.exe"
sc.exe start MDTWebUI
```

### Using NSSM (Non-Sucking Service Manager)

```powershell
nssm install MDTWebUI "C:\path\to\publish\MDT.WebUI.exe"
nssm start MDTWebUI
```

## 5. Linux Systemd Service

### Create Service File

Create `/etc/systemd/system/mdt-webui.service`:

```ini
[Unit]
Description=Modern Deployment Toolkit Web API
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/mdt-webui
ExecStart=/usr/bin/dotnet /opt/mdt-webui/MDT.WebUI.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=mdt-webui
User=mdt
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

### Deploy

```bash
# Copy files
sudo mkdir -p /opt/mdt-webui
sudo cp -r ./publish/* /opt/mdt-webui/

# Create user
sudo useradd -r -s /bin/false mdt
sudo chown -R mdt:mdt /opt/mdt-webui

# Enable and start service
sudo systemctl enable mdt-webui
sudo systemctl start mdt-webui
sudo systemctl status mdt-webui
```

## 6. IIS Deployment (Windows)

### Prerequisites
- IIS with ASP.NET Core Hosting Bundle

### Deploy Steps

1. Publish the application:
```bash
dotnet publish MDT.WebUI -c Release -o ./publish
```

2. Create IIS Application Pool:
   - No Managed Code
   - Integrated Pipeline Mode

3. Create IIS Site:
   - Point to publish directory
   - Assign Application Pool
   - Configure bindings

4. Set permissions:
   - Grant IIS_IUSRS read access to site directory

5. Configure `web.config` (auto-generated):
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\MDT.WebUI.dll" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

## 7. Azure Deployment

### Azure App Service

```bash
# Create Resource Group
az group create --name mdt-rg --location eastus

# Create App Service Plan
az appservice plan create --name mdt-plan \
  --resource-group mdt-rg \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create --name mdt-webapp \
  --resource-group mdt-rg \
  --plan mdt-plan \
  --runtime "DOTNETCORE:8.0"

# Deploy
cd MDT.WebUI
az webapp up --name mdt-webapp --resource-group mdt-rg
```

### Azure Container Instances

```bash
# Create Container Instance
az container create \
  --resource-group mdt-rg \
  --name mdt-container \
  --image mdt-webui:latest \
  --dns-name-label mdt-api \
  --ports 80
```

### Azure Kubernetes Service (AKS)

```bash
# Create AKS Cluster
az aks create \
  --resource-group mdt-rg \
  --name mdt-cluster \
  --node-count 3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# Get credentials
az aks get-credentials --resource-group mdt-rg --name mdt-cluster

# Deploy
kubectl apply -f mdt-deployment.yaml
```

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)
- `Database__UseSqlite`: Use SQLite (true/false)
- `ConnectionStrings__DefaultConnection`: Database connection string

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Database": {
    "UseSqlite": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mdt;Username=mdt;Password=yourpassword"
  }
}
```

## Database Setup

### SQLite (Default)
- Automatic creation on first run
- Database file: `mdt.db`

### PostgreSQL

```sql
CREATE DATABASE mdt;
CREATE USER mdt WITH PASSWORD 'yourpassword';
GRANT ALL PRIVILEGES ON DATABASE mdt TO mdt;
```

## Monitoring and Logging

### Application Insights (Azure)

Add to `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### Serilog

Add NuGet package and configure in `Program.cs`.

## Security Considerations

### Production Checklist

- [ ] Enable HTTPS only
- [ ] Configure CORS properly
- [ ] Add authentication/authorization
- [ ] Use secure connection strings
- [ ] Enable request rate limiting
- [ ] Configure firewall rules
- [ ] Use secure database passwords
- [ ] Enable logging and monitoring
- [ ] Keep dependencies updated
- [ ] Run as non-privileged user

### HTTPS Configuration

Update `appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "cert-password"
        }
      }
    }
  }
}
```

## Troubleshooting

### Check Logs

```bash
# Docker
docker logs mdt-webui

# Systemd
sudo journalctl -u mdt-webui -f

# Windows
Check Event Viewer
```

### Common Issues

1. **Database Connection Fails**
   - Verify connection string
   - Check database server is running
   - Verify firewall rules

2. **Port Already in Use**
   - Change port in configuration
   - Stop conflicting service

3. **Permission Denied**
   - Check file permissions
   - Run with appropriate user

## Scaling

### Horizontal Scaling
- Deploy multiple instances behind load balancer
- Use shared database (PostgreSQL recommended)
- Configure session state for distributed cache

### Vertical Scaling
- Increase container resources
- Upgrade hosting plan/VM size

## Backup and Recovery

### Database Backup

#### SQLite
```bash
sqlite3 mdt.db ".backup backup.db"
```

#### PostgreSQL
```bash
pg_dump mdt > backup.sql
```

### Application Backup
- Back up configuration files
- Back up task sequence files
- Back up execution logs

## Support

For deployment issues, please:
1. Check logs for error messages
2. Review this guide
3. Open an issue on GitHub with details
