
# Firebase Realtime Database Structure Documentation

## Overview
This document outlines the database structure for a chat application similar to Slack, designed using Firebase Realtime Database.

## Database Schema

### 1. Channel Table
Path: `/channels/{channelId}`
- **ID** (GUID): Unique identifier for the channel.
- **Name** (String): Name of the channel.
- **Description** (Text): Brief description of the channel.
- **Visibility** (Enum): Access level of the channel, can be 'Private', 'Internal', or 'Public'.
- **CreatedBy** (GUID): ID of the user who created the channel.
- **Type** (String): Type of the channel, e.g., 'text', 'voice'.
- **Tag** (String): Additional categorization tag.
- **CreatedAt** (Timestamp): Timestamp of when the channel was created.

### 2. Member Table
Path: `/members/{memberId}`
- **ID** (GUID): Unique identifier for the member.
- **Name** (String): Real name of the member.
- **Nickname** (String): Nickname or alias in the system.
- **Type** (String): Type of member, e.g., 'admin', 'user'.
- **Tag** (String): Tag for additional categorization.
- **CreatedAt** (Timestamp): Timestamp of when the member was added.

### 3. Session Members Table
Path: `/sessionMembers/{sessionId}_{memberId}`
- **SessionID** (GUID): Identifier of the session.
- **MemberID** (GUID): Identifier of the member.
- **Type** (String): Type of session.
- **Tag** (String): Tag for additional information or filtering.
- **IsAdmin** (Boolean): Boolean flag to indicate if the member is an admin.
- **IsModerator** (Boolean): Boolean flag to indicate if the member is a moderator.
- **AccessKey** (String): Special key for session access, if necessary.
- **Role** (String): Role of the member within the session.
- **IsActive** (Boolean): Status if the member is currently active in the session.
- **Ordinal** (Integer): Ordering of the member within the session.
- **CreatedAt** (Timestamp): Timestamp when the member joined the session.
- **Position** (Binary Array): Custom binary array for storing structured position data.

### 4. Message Table
Path: `/messages/{messageId}`
- **ID** (GUID): Unique identifier for the message.
- **Text** (Text): Content of the message.
- **Payload** (Text): Any additional data attached to the message.
- **ParentID** (GUID): ID of the parent message, for thread handling.
- **BindedMessageID** (GUID): ID of the binded message; showed as an attached message, ex: helper text, text to Audio ....
- **IsHidden** (Boolean): Flag to hide the message from normal view.
- **IsDeleted** (Boolean): Flag to hide the content of the message and mark it as deleted.
- **IsEdited** (Boolean): Flag to mark the message as deleted.
- **MessageLogs** (Text): JSON array showing the logs of the message (Edited, Deleted, ...)
- **IsMaskedText** (Boolean): Flag to indicate text masking for privacy.
- **DeliveredTo** (Binary Array): Array of member IDs to whom the message has been delivered.
- **SeenBy** (Binary Array): Array of member IDs who have seen the message.
- **SessionID** (GUID): Identifier of the session where the message was sent.
- **CreatedAt** (Timestamp): Timestamp when the message was sent.

## Security Rules

```json
{
  "rules": {
    "channels": {
      "$channelId": {
        ".read": "auth != null && data.child('Visibility').val() === 'Public'",
        ".write": "auth != null && data.child('CreatedBy').val() === auth.uid"
      }
    },
    "members": {
      "$memberId": {
        ".read": "auth != null && auth.uid == $memberId",
        ".write": "auth != null && auth.uid == $memberId"
      }
    },
    "sessionMembers": {
      "$sessionMemberId": {
        ".read": "auth != null && data.child('MemberID').val() === auth.uid",
        ".write": "auth != null && data.child('MemberID').val() === auth.uid"
      }
    },
    "messages": {
      "$messageId": {
        ".read": "auth != null && data.child('SessionID').val() in auth.sessions",
        ".write": "auth != null && data.child('SessionID').val() in auth.sessions"
      }
    }
  }
}
```
