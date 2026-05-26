# MongoDB Setup for PollPulse

## Local MongoDB Setup

1. Install MongoDB Community Server.
2. Start MongoDB service.
3. Keep the default local URL:

```text
mongodb://127.0.0.1:27017
```

4. The backend uses this database name:

```text
au_cs_voice_portal
```

5. Run the backend. The database is created automatically on first run.

```bash
cd backend
dotnet restore
dotnet run --launch-profile https
```

## Connection String Location

The backend connection string is stored in:

```text
backend/appsettings.json
```

```json
"ConnectionStrings": {
  "MongoDB": "mongodb://127.0.0.1:27017/au_cs_voice_portal"
}
```

## Seed Data

Seed data is inserted by:

```text
backend/Services/MongoDBService.cs
```

The seed method creates admins, students, classes, teachers, courses, events, elections, candidates, votes, feedback and audit logs.

## Reset Database for Fresh Demo

Open MongoDB shell or Compass and drop the database:

```javascript
use au_cs_voice_portal
db.dropDatabase()
```

Then run the backend again. The seed data will be created again.
