# MongoDB Schema - PollPulse

PollPulse uses MongoDB collections. Relationships are maintained using ObjectId string references.

## Collections

### admins
Stores admin users.

Fields: `_id`, `name`, `email`, `passwordHash`, `role`, `status`, `lastLoginAt`, `createdAt`, `updatedAt`

### users
Stores student accounts.

Fields: `_id`, `name`, `email`, `rollNo`, `degree`, `year`, `section`, `classKey`, `passwordHash`, `role`, `status`, `lastLoginAt`, `createdAt`, `updatedAt`

### classsections
Stores academic class sections.

Fields: `_id`, `degree`, `year`, `section`, `classKey`, `title`, `isActive`, `createdAt`, `updatedAt`

### teachers
Stores teacher master data.

Fields: `_id`, `name`, `department`, `email`, `isActive`, `createdAt`, `updatedAt`

### courses
Stores course master data.

Fields: `_id`, `code`, `title`, `degree`, `year`, `isActive`, `createdAt`, `updatedAt`

### events
Stores event master data.

Fields: `_id`, `name`, `eventDate`, `description`, `status`, `createdAt`, `updatedAt`

### elections
Stores CR/GR elections.

Fields: `_id`, `title`, `description`, `electionType`, `degree`, `year`, `section`, `classKey`, `status`, `startAt`, `endAt`, `createdBy`, `createdAt`, `updatedAt`

Relationships:

- `createdBy` references `admins._id`
- `classKey` connects election to student class

### candidates
Stores election candidates.

Fields: `_id`, `election`, `name`, `rollNo`, `manifesto`, `order`, `isActive`, `createdAt`, `updatedAt`

Relationships:

- `election` references `elections._id`

### votes
Stores student votes.

Fields: `_id`, `election`, `candidate`, `user`, `classKey`, `electionType`, `ipAddress`, `userAgent`, `createdAt`, `updatedAt`

Relationships:

- `election` references `elections._id`
- `candidate` references `candidates._id`
- `user` references `users._id`

Important rule: one student can vote only once per election. This is enforced by a unique index on `election + user`.

### feedbacks
Stores category-wise student feedback.

Fields: `_id`, `user`, `category`, `targetName`, `teacher`, `course`, `event`, `ratings`, `averageRating`, `comment`, `suggestion`, `sentiment`, `degree`, `year`, `section`, `classKey`, `createdAt`, `updatedAt`

Relationships:

- `user` references `users._id`
- `teacher` references `teachers._id` when category is teacher
- `course` references `courses._id` when category is course
- `event` references `events._id` when category is event

### auditlogs
Stores backend activity history.

Fields: `_id`, `actor`, `actorModel`, `actorRole`, `action`, `entityType`, `entityId`, `details`, `ipAddress`, `createdAt`, `updatedAt`

## Indexes

The backend creates these indexes automatically:

| Collection | Index | Purpose |
|---|---|---|
| users | unique email | Prevent duplicate student accounts |
| admins | unique email | Prevent duplicate admin accounts |
| votes | unique election + user | Prevent duplicate votes |
| elections | classKey + status | Faster class election lookup |
| feedbacks | user + category + createdAt | Faster feedback history lookup |
