
# Chatting Microservice README

## Overview
This document details the Chatting Microservice, designed to facilitate robust chat functionalities in applications. It handles a wide range of operations related to messaging including member management, channel management, and message handling.

## Purpose
The aim of the Chatting Microservice is to provide a comprehensive solution for real-time messaging needs, supporting direct messages, group chats, and broadcast messages.

## Features & API Endpoints

| Feature Category | Operation | API Endpoint | Description |
|------------------|-----------|--------------|-------------|
| Members | Create Member | `POST /members` | Create a new member in the chat application. |
| Members | Deactivate Member | `PATCH /members/{memberId}/deactivate` | Deactivate a member, disabling their access. |
| Channels | Create Channel | `POST /channels` | Create a new channel for communication. |
| Channels | Add Members to Channel | `POST /channels/{channelId}/members` | Add members to an existing channel. |
| Channels | Deactivate Channel | `PATCH /channels/{channelId}/deactivate` | Deactivate a channel, stopping all activity. |
| Messages | Add Message | `POST /channels/{channelId}/messages` | Add a message to a channel. |
| Messages | Reply to Message | `POST /channels/{channelId}/messages/{messageId}/reply` | Reply to an existing message in a channel. |
| Messages | Bind Message | `POST /channels/{channelId}/messages/{messageId}/bind` | Attach a message to another as a context. |
| Messages | Delete Message | `DELETE /channels/{channelId}/messages/{messageId}` | Remove a message from the channel. |
| Messages | Mask Message | `PATCH /channels/{channelId}/messages/{messageId}/mask` | Mask the content of a message for privacy. |
| Messages | Hide Message | `PATCH /channels/{channelId}/messages/{messageId}/hide` | Hide a message from the general view. |
| Messages | Unmask Message | `PATCH /channels/{channelId}/messages/{messageId}/unmask` | Reverse the masking of a message. |
| Messages | Mark Message as Delivered | `POST /channels/{channelId}/messages/{messageId}/delivered` | Record a message as delivered to specific members. |
| Messages | Mark Message as Seen | `POST /channels/{channelId}/messages/{messageId}/seen` | Mark a message as seen by specific members. |

## Documentation Links
- [Full API Specifications](documentation/openapi.yaml)
- [Messaging Process Overview](documentation/Messaging_Process_Flow_Documentation.md)
- [Database Schema Details](documentation/UML_Class_Diagram.mmd)
- [Firebase RealTime Database Documentation](Firebase_Realtime_Database_Documentation.md)

## Setup and Deployment
Follow the setup instructions as outlined in the linked documentation to configure and deploy the microservice.

## Contributing
Contributions are welcome. Senior developers are encouraged to improve the features and functionalities of the microservice. See the GitHub repository for contribution guidelines.

## Additional Resources
For more detailed operational workflows and integration with other services, refer to the comprehensive documentation available on our GitHub repository: [GitHub Main Repository](https://github.com/example/microservice-chatting).
