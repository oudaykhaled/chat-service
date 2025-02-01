﻿using Microsoft.AspNetCore.Mvc;

namespace ChatService.Domain.Request
{
    public class DeactivateMemberRequest
    {
        [FromRoute(Name = "memberId")]
        public string ID { get; set; }
    }
}
