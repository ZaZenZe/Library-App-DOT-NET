# Azure Deployment Instructions

## Changes Made to Fix Deployment

### 1. GitHub Actions Workflow (`.github/workflows/LibraryAppRickySayan.yml`)
- ? Fixed publish path from `"Library App/publish"` to `"./publish"` to avoid path issues on Linux runners
- ? Added Azure logout step at the end of deployment
- ? Improved artifact handling between build and deploy jobs

### 2. Web Configuration (`Library App/web.config`)
- ? Created `web.config` file for Azure App Service to properly route requests to your .NET 9 app
- ? Configured ASP.NET Core Module V2 with in-process hosting for better performance

### 3. Application Startup (`Library App/Program.cs`)
- ? Made connection string requirement less strict during startup
- ? Added proper warning messages when DB is not configured
- ? Allows the app to start even without a database connection (will show health check page)

### 4. Project Configuration (`Library App/Library App.csproj`)
- ? Added `web.config` to be included in published output

## Steps to Complete Azure Deployment

### Step 1: Configure Connection String in Azure
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your App Service: **LibraryAppRickySayan**
3. Go to **Configuration** ? **Application settings**
4. Click **+ New connection string**
5. Add the following:
   - **Name**: `Library_AppContextConnection`
   - **Value**: Your Azure SQL connection string (e.g., `Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=yourdb;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`)
   - **Type**: `SQLAzure`
6. Click **OK** and then **Save**

### Step 2: Configure Azure SQL Firewall (if using Azure SQL)
1. In Azure Portal, go to your SQL Server resource
2. Go to **Security** ? **Networking**
3. Under **Firewall rules**, check **Allow Azure services and resources to access this server**
4. Click **Save**

### Step 3: Deploy Your Application
1. Commit and push your changes to the `master` branch:
   ```bash
   git add .
   git commit -m "Fix Azure deployment configuration"
 git push origin master
   ```

2. The GitHub Actions workflow will automatically trigger and deploy your app

3. Wait 5-10 minutes for the deployment to complete

### Step 4: Verify Deployment
After deployment completes, visit your URLs:

- **Health Check**: https://libraryapprickysayan.azurewebsites.net/
  - Should show: `"Server is running"`

- **Ping**: https://libraryapprickysayan.azurewebsites.net/ping
  - Should show: `"pong"`

- **Swagger UI** (if in Development mode): https://libraryapprickysayan.azurewebsites.net/swagger

- **Get All Books**: https://libraryapprickysayan.azurewebsites.net/books

## Troubleshooting

### App shows "Your web app is running and waiting for your content"
- **Cause**: The deployment hasn't completed yet, or files weren't published correctly
- **Solution**: Wait 5 minutes, then check GitHub Actions logs for errors

### 500 Internal Server Error
- **Cause**: Missing connection string or database connection issues
- **Solution**: Check Application Insights or Log Stream in Azure Portal for detailed errors

### Database Connection Errors
- **Cause**: Connection string not configured or SQL firewall blocking connections
- **Solution**: 
  1. Verify connection string in Azure App Service Configuration
  2. Check SQL Server firewall rules
  3. Enable "Allow Azure services" in SQL Server settings

### View Application Logs
1. In Azure Portal, go to your App Service
2. Click **Log stream** in the left menu
3. Watch for connection string debug output and any error messages

## Testing Your API

Once deployed successfully, you can test your API endpoints:

```bash
# Health check
curl https://libraryapprickysayan.azurewebsites.net/

# Get all books
curl https://libraryapprickysayan.azurewebsites.net/books

# Get book by ID
curl https://libraryapprickysayan.azurewebsites.net/books/1

# Create a new book (requires proper JSON)
curl -X POST https://libraryapprickysayan.azurewebsites.net/books \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Book","isbn":"978-1234567890","year":2024,"authorId":1}'
```

## Important Notes

- The app will start even without a database connection, allowing you to see health check endpoints
- Database migrations run automatically on startup when a connection string is configured
- The connection string debug output will show in logs (with sensitive parts hidden)
- Make sure to set `ASPNETCORE_ENVIRONMENT` to `Production` in Azure for production deployments
