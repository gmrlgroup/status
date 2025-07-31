# Status Management Public API

This API provides external services with the ability to create and manage incidents and entity status history for entities in the status management system.

## Overview

The Public API allows external monitoring systems, alerting tools, and other services to:
- Create new incidents when issues are detected
- Update incident status as issues are investigated and resolved
- Retrieve incident information
- Resolve incidents with detailed resolution information
- Track entity status history over time
- Record uptime and performance metrics
- Query historical status data

## Base URL

```
https://localhost:7000/api
```

## Authentication

Currently, the API is open for development. In production, you should implement API key authentication or other security measures.

## Endpoints

### Incidents API

### POST /incidents
Creates a new incident for a specific entity.

**Request Body:**
```json
{
  "entityId": "string (required)",
  "title": "string (required, max 200 chars)",
  "description": "string (required, max 4000 chars)",
  "severity": "integer (1=Low, 2=Medium, 3=High, 4=Critical)",
  "reportedBy": "string (optional, max 200 chars)",
  "assignedTo": "string (optional, max 200 chars)",
  "impactDescription": "string (optional, max 1000 chars)",
  "externalIncidentId": "string (optional, max 100 chars)",
  "metadata": "string (optional JSON, max 4000 chars)",
  "startedAt": "datetime (optional, defaults to current UTC time)"
}
```

**Response (201 Created):**
```json
{
  "id": "string",
  "title": "string",
  "status": "integer",
  "severity": "integer",
  "createdAt": "datetime",
  "startedAt": "datetime",
  "externalIncidentId": "string"
}
```

### GET /incidents/{id}
Retrieves an incident by its ID.

**Response (200 OK):**
Returns the complete incident object with all details.

### PUT /incidents/{id}/status
Updates the status of an incident.

**Query Parameters:**
- `message` (optional): Status update message
- `updatedBy` (optional): Who is updating the status

**Request Body:**
```json
integer (1=Open, 2=Investigating, 3=Identified, 4=Monitoring, 5=Resolved)
```

### PUT /incidents/{id}/resolve
Marks an incident as resolved.

**Query Parameters:**
- `resolvedBy` (optional): Who resolved the incident

**Request Body:**
```json
"string (required): Details about how the incident was resolved"
```

### Entity Status History API

### POST /entitystatushistory
Creates a new entity status history record for monitoring and tracking.

**Request Body:**
```json
{
  "entityId": "string (required)",
  "status": "integer (required: 0=Unknown, 1=Online, 2=Offline, 3=Degraded, 4=Maintenance, 5=Error)",
  "statusMessage": "string (optional, max 2000 chars)",
  "responseTime": "number (optional, milliseconds)",
  "uptimePercentage": "number (optional, 0-100)",
  "checkedAt": "datetime (optional, defaults to current UTC time)"
}
```

**Response (201 Created):**
```json
{
  "id": "integer",
  "entityId": "string",
  "status": "integer",
  "statusMessage": "string",
  "responseTime": "number",
  "uptimePercentage": "number",
  "checkedAt": "datetime",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET /entitystatushistory/{id}
Retrieves a specific entity status history record by ID.

### GET /entitystatushistory/entity/{entityId}
Retrieves entity status history for a specific entity with filtering and pagination.

**Query Parameters:**
- `fromDate` (optional): Start date for filtering
- `toDate` (optional): End date for filtering
- `status` (optional): Filter by specific status value
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Records per page (default: 50, max: 1000)

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "integer",
      "entityId": "string",
      "status": "integer",
      "statusMessage": "string",
      "responseTime": "number",
      "uptimePercentage": "number",
      "checkedAt": "datetime",
      "createdAt": "datetime",
      "updatedAt": "datetime"
    }
  ],
  "totalCount": "integer",
  "page": "integer",
  "pageSize": "integer",
  "totalPages": "integer",
  "hasNextPage": "boolean",
  "hasPreviousPage": "boolean"
}
```

### PUT /entitystatushistory/{id}
Updates an existing entity status history record.

**Request Body:**
```json
{
  "status": "integer (required)",
  "statusMessage": "string (optional, max 2000 chars)",
  "responseTime": "number (optional, milliseconds)",
  "uptimePercentage": "number (optional, 0-100)",
  "checkedAt": "datetime (required)"
}
```

### DELETE /entitystatushistory/{id}
Soft deletes an entity status history record.

**Response (204 No Content):**
Returns empty response on successful deletion.

### GET /entitystatushistory/entity/{entityId}/summary
Retrieves summary statistics for an entity's status history.

**Query Parameters:**
- `fromDate` (optional): Start date for statistics
- `toDate` (optional): End date for statistics

**Response (200 OK):**
```json
{
  "entityId": "string",
  "statusCounts": {
    "1": 150,
    "2": 5,
    "3": 10,
    "5": 2
  },
  "averageResponseTime": "number",
  "averageUptime": "number",
  "totalChecks": "integer",
  "fromDate": "datetime",
  "toDate": "datetime"
}
```

## Status and Severity Values

### Incident Severity Levels

- **1 - Low**: Minor issues with minimal impact
- **2 - Medium**: Moderate issues affecting some users
- **3 - High**: Significant issues affecting many users
- **4 - Critical**: Severe issues affecting all users or critical systems

### Incident Status Values

- **1 - Open**: Incident has been reported but not yet addressed
- **2 - Investigating**: Team is actively investigating the issue
- **3 - Identified**: Root cause has been identified
- **4 - Monitoring**: Fix has been applied and monitoring for stability
- **5 - Resolved**: Issue has been completely resolved

### Entity Status Values

- **0 - Unknown**: Status is unknown or undetermined
- **1 - Online**: Entity is fully operational
- **2 - Offline**: Entity is completely unavailable
- **3 - Degraded**: Entity is operational but with reduced performance
- **4 - Maintenance**: Entity is under planned maintenance
- **5 - Error**: Entity has encountered an error and may be partially unavailable

## Error Responses

All endpoints return standardized error responses:

```json
{
  "message": "string",
  "details": "string (optional)",
  "validationErrors": "object (optional)",
  "timestamp": "datetime"
}
```

Common HTTP status codes:
- **400 Bad Request**: Invalid request data or validation errors
- **404 Not Found**: Requested resource not found
- **500 Internal Server Error**: Server-side error occurred

## Examples

See the `examples.http` and `entity-status-history-examples.http` files for complete examples of how to use each endpoint.

### Creating a Critical Incident

```bash
curl -X POST "https://localhost:7000/api/incidents" \
  -H "Content-Type: application/json" \
  -d '{
    "entityId": "db-primary-001",
    "title": "Database Connection Pool Exhausted",
    "description": "All database connections are in use, new requests are being rejected",
    "severity": 4,
    "reportedBy": "Monitoring System",
    "assignedTo": "Database Team",
    "impactDescription": "100% of users cannot access the application",
    "externalIncidentId": "MON-2025-001"
  }'
```

### Recording Entity Status

```bash
curl -X POST "https://localhost:7000/api/entitystatushistory" \
  -H "Content-Type: application/json" \
  -d '{
    "entityId": "web-server-001",
    "status": 1,
    "statusMessage": "HTTP 200 OK - All endpoints responding",
    "responseTime": 95.2,
    "uptimePercentage": 100.0
  }'
```

### Querying Entity Status History

```bash
curl -X GET "https://localhost:7000/api/entitystatushistory/entity/web-server-001?fromDate=2025-01-01T00:00:00Z&toDate=2025-01-24T23:59:59Z&page=1&pageSize=50"
```

### Getting Entity Status Summary

```bash
curl -X GET "https://localhost:7000/api/entitystatushistory/entity/web-server-001/summary?fromDate=2025-01-01T00:00:00Z"
```

### Updating Incident Status

```bash
curl -X PUT "https://localhost:7000/api/incidents/{incident-id}/status?message=Investigating+connection+pool+settings&updatedBy=DevOps+Team" \
  -H "Content-Type: application/json" \
  -d '2'
```

### Resolving an Incident

```bash
curl -X PUT "https://localhost:7000/api/incidents/{incident-id}/resolve?resolvedBy=Database+Team" \
  -H "Content-Type: application/json" \
  -d '"Increased connection pool size and implemented connection retry logic. System is stable."'
```

## Integration Notes

### Incidents
1. **Entity ID Validation**: Ensure the `entityId` exists in the system before creating incidents
2. **External ID Tracking**: Use `externalIncidentId` to track incidents in your external systems
3. **Metadata Usage**: Store additional context as JSON in the `metadata` field
4. **Error Handling**: Always check response status codes and handle errors appropriately
5. **Timestamps**: All timestamps are in UTC format

### Entity Status History
1. **Regular Monitoring**: External monitoring systems should regularly record status checks
2. **Response Time Tracking**: Include response times in milliseconds for performance monitoring
3. **Uptime Calculations**: Record uptime percentages for availability tracking
4. **Status Messages**: Use descriptive status messages for troubleshooting context
5. **Bulk Operations**: Consider batching status updates for high-frequency monitoring
6. **Data Retention**: Plan for data retention as status history can grow quickly
7. **Time Zones**: All timestamps should be in UTC format
8. **Pagination**: Use pagination for large datasets when querying historical data

## Development

To run the API locally:

1. Ensure the database connection string is configured in `appsettings.json`
2. Run database migrations if needed
3. Start the application: `dotnet run --project Application.Api`
4. Access Swagger documentation at: `https://localhost:7000`

## Security Considerations

For production deployment:
- Implement API key authentication
- Add rate limiting
- Enable HTTPS only
- Validate and sanitize all inputs
- Implement audit logging
- Consider IP whitelisting for trusted external services
