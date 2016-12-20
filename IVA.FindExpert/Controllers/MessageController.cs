﻿using IVA.Common;
using IVA.DbAccess;
using IVA.DbAccess.Repository;
using IVA.DTO;
using IVA.FindExpert.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IVA.FindExpert.Controllers
{
    public class MessageController : BaseController
    {

        [HttpGet]
        //[Authorize]
        public IHttpActionResult GetThreadById(long ThreadId)
        {
            MessageThreadModel thread = null;
            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    var t = new MessageThreadRepository(context).GetById(ThreadId);
                    if(t != null)
                    {
                        thread = new MessageThreadModel
                        {
                            Id = t.Id,
                            AgentId = t.AgentId,
                            BuyerId = t.BuyerId,
                            RequestId = t.RequestId,
                            Date = t.CreatedTime.GetAdjustedTime().ToString("dd/MMM"),
                            Time = t.CreatedTime.GetAdjustedTime().ToString("HH:mm"),
                            Messages = t.Messages.OrderByDescending(
                            m => m.Time).Select(m => new MessageModel
                            {
                                Id = m.Id,
                                ThreadId = m.ThreadId,
                                RecieverId = m.RecieverId,
                                SenderId = m.SenderId,
                                MessageText = m.MessageText,
                                Status = m.Status,
                                Time = m.Time.GetAdjustedTime().ToString("yyyy-MM-dd hh:mm tt")
                            }).ToList()

                        };

                        var userRepo = new UserRepository(context);
                        var userProfileRepo = new UserProfileRepository(context);
                        var buyer = userRepo.GetByUserId(thread.BuyerId);
                        var agent = userRepo.GetByUserId(thread.AgentId);
                        var buyerProfile = userProfileRepo.GetByUserId(thread.BuyerId);
                        var agentProfile = userProfileRepo.GetByUserId(thread.AgentId);
                        var request = new ServiceRequestRepository(context).GetById(thread.RequestId);
                        thread.AgentName = agent.Name;
                        if (agentProfile != null)
                            thread.AgentName = agentProfile.FirstName + " " + agentProfile.LastName;
                        thread.CompanyName = agent.Company.Name;
                        thread.BuyerName = buyer.Name;
                        if (buyerProfile != null)
                            thread.BuyerName = buyerProfile.FirstName + " " + buyerProfile.LastName;

                        thread.Description = "Vehicle No: " + request.VehicleNo + " / Request: " + request.Code;

                        foreach (var message in thread.Messages)
                        {
                            var sender = userRepo.GetByUserId(message.SenderId);
                            message.SenderName = sender.Name;
                            var senderProfile = userProfileRepo.GetByUserId(message.SenderId);
                            if (senderProfile != null)
                                message.SenderName = senderProfile.FirstName + " " + senderProfile.LastName;
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(thread);
        }


        [HttpGet]
        [Authorize]
        public IHttpActionResult GetBuyerThreads(long UserId)
        {
            List<MessageThreadModel> threads = null;
            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    var userThreads = new MessageThreadRepository(context).GetByBuyerId(UserId).OrderByDescending(t => t.CreatedTime);
                    threads = userThreads.Select(t => new MessageThreadModel
                    {
                        Id = t.Id,
                        AgentId = t.AgentId,
                        BuyerId = t.BuyerId,  
                        RequestId = t.RequestId,                      
                        Date = t.CreatedTime.GetAdjustedTime().ToString("dd/MMM"),
                        Time = t.CreatedTime.GetAdjustedTime().ToString("HH:mm"),
                        Messages = t.Messages.OrderByDescending(
                            m => m.Time).Select(m => new MessageModel
                            {
                                Id = m.Id,
                                ThreadId = m.ThreadId,
                                RecieverId = m.RecieverId,
                                SenderId = m.SenderId,                                
                                MessageText = m.MessageText,
                                Status = m.Status,
                                Time = m.Time.GetAdjustedTime().ToString("yyyy-MM-dd HH:mm")
                            }).ToList()

                    }).ToList();

                    foreach(var thread in threads)
                    {
                        var userRepo = new UserRepository(context);
                        var userProfileRepo = new UserProfileRepository(context);
                        var buyer = userRepo.GetByUserId(thread.BuyerId);
                        var agent = userRepo.GetByUserId(thread.AgentId);
                        var buyerProfile = userProfileRepo.GetByUserId(thread.BuyerId);
                        var agentProfile = userProfileRepo.GetByUserId(thread.AgentId);
                        var request = new ServiceRequestRepository(context).GetById(thread.RequestId);
                        thread.AgentName = agent.Name;
                        if (agentProfile != null)
                            thread.AgentName = agentProfile.FirstName + " " + agentProfile.LastName;
                        thread.CompanyName = agent.Company.Name;
                        thread.BuyerName = buyer.Name;
                        if (buyerProfile != null)
                            thread.BuyerName = buyerProfile.FirstName + " " + buyerProfile.LastName;

                        thread.Description = "Vehicle No: " + request.VehicleNo + " / Request: " + request.Code;
                        thread.VehicleNo = request.VehicleNo;

                        foreach(var message in thread.Messages)
                        {
                            var sender = userRepo.GetByUserId(message.SenderId);
                            message.SenderName = sender.Name;
                            var senderProfile = userProfileRepo.GetByUserId(message.SenderId);
                            if (senderProfile != null)
                                message.SenderName = senderProfile.FirstName + " " + senderProfile.LastName;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(threads);
        }

        [HttpGet]
        //[Authorize]
        public IHttpActionResult GetAgentThreads(long UserId)
        {
            List<MessageThreadModel> threads = null;
            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    var userThreads = new MessageThreadRepository(context).GetByAgentId(UserId).OrderByDescending(t => t.CreatedTime);
                    threads = userThreads.Select(t => new MessageThreadModel
                    {
                        Id = t.Id,
                        AgentId = t.AgentId,
                        BuyerId = t.BuyerId,
                        RequestId = t.RequestId,
                        Date = t.CreatedTime.GetAdjustedTime().ToString("dd/MMM"),
                        Time = t.CreatedTime.GetAdjustedTime().ToString("HH:mm"),
                        Messages = t.Messages.OrderByDescending(
                            m => m.Time).Select(m => new MessageModel
                            {
                                Id = m.Id,
                                ThreadId = m.ThreadId,
                                RecieverId = m.RecieverId,
                                SenderId = m.SenderId,
                                MessageText = m.MessageText,
                                Status = m.Status,
                                Time = m.Time.GetAdjustedTime().ToString("yyyy-MM-dd HH:mm")
                            }).ToList()

                    }).ToList();

                    foreach (var thread in threads)
                    {
                        var userRepo = new UserRepository(context);
                        var userProfileRepo = new UserProfileRepository(context);
                        var buyer = userRepo.GetByUserId(thread.BuyerId);
                        var agent = userRepo.GetByUserId(thread.AgentId);
                        var buyerProfile = userProfileRepo.GetByUserId(thread.BuyerId);
                        var agentProfile = userProfileRepo.GetByUserId(thread.AgentId);
                        var request = new ServiceRequestRepository(context).GetById(thread.RequestId);
                        thread.AgentName = agent.Name;
                        if (agentProfile != null)
                            thread.AgentName = agentProfile.FirstName + " " + agentProfile.LastName;
                        thread.CompanyName = agent.Company.Name;
                        thread.BuyerName = buyer.Name;
                        if (buyerProfile != null)
                            thread.BuyerName = buyerProfile.FirstName + " " + buyerProfile.LastName;

                        thread.Description = "Vehicle No: " + request.VehicleNo + " / Request: " + request.Code;
                        thread.VehicleNo = request.VehicleNo;

                        foreach (var message in thread.Messages)
                        {
                            var sender = userRepo.GetByUserId(message.SenderId);
                            message.SenderName = sender.Name;
                            var senderProfile = userProfileRepo.GetByUserId(message.SenderId);
                            if (senderProfile != null)
                                message.SenderName = senderProfile.FirstName + " " + senderProfile.LastName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(threads);
        }


        [HttpPost]
        [Authorize]
        public IHttpActionResult AddThread(MessageThreadModel Model)
        {
            long id = 0;
            try
            {
                MessageThread thread = new MessageThread();
                thread.AgentId = Model.AgentId;
                thread.BuyerId = Model.BuyerId;
                thread.RequestId = Model.RequestId;
                thread.CreatedBy = Model.CreatedBy;
                thread.CreatedTime = DateTime.Now.ToUniversalTime();

                using (AppDBContext context = new AppDBContext())
                {
                    var repo = new MessageThreadRepository(context);
                    id = repo.SafeAdd(thread);
                }
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(id);            
        }

        [HttpPost]
        [Authorize]
        public IHttpActionResult AddMessage(MessageModel Model)
        {
            long Id = 0;
            try
            {
                Message message = new Message
                {
                    Id = Model.Id,
                    ThreadId = Model.ThreadId,
                    MessageText = Model.MessageText,
                    SenderId = Model.SenderId,
                    RecieverId = Model.RecieverId,
                    Status = (int)Constant.MessageStatus.Initial,
                    Time = DateTime.Now.ToUniversalTime()
                };

                using (AppDBContext context = new AppDBContext())
                {
                    if(message.ThreadId == 0)
                    {
                        var userRepo = new UserRepository(context);
                        var sender = userRepo.GetByUserId(message.SenderId);
                        var recipient = userRepo.GetByUserId(message.RecieverId);
                        long agentId = 0;
                        long buyerId = 0;
                        if (sender.UserType == Constant.UserType.BUYER)
                        {
                            buyerId = sender.Id;
                            agentId = recipient.Id;
                        }
                        else
                        {
                            buyerId = recipient.Id;
                            agentId = sender.Id;
                        }

                        var existingthread =
                            new MessageThreadRepository(context).GetByAgentId(agentId).Where(
                                t => t.RequestId == Model.RequestId && t.BuyerId == buyerId).FirstOrDefault();

                        if(existingthread == null)
                        {
                            MessageThread thread = new MessageThread();
                            thread.AgentId = agentId;
                            thread.BuyerId = buyerId;
                            thread.RequestId = Model.RequestId;
                            thread.CreatedBy = Model.SenderId;
                            thread.CreatedTime = DateTime.Now.ToUniversalTime();

                            var repo = new MessageThreadRepository(context);
                            long tid = repo.Add(thread);
                            message.ThreadId = tid;
                        }
                        else
                        {
                            message.ThreadId = existingthread.Id;
                        }                        
                    }

                    Id = new MessageRepository(context).Add(message);
                    
                }
            }
            catch(Exception ex)
            {
                InternalServerError(ex);
            }

            return Ok(Id);
        }


    }
}