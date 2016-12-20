﻿using IVA.Common;
using IVA.DbAccess;
using IVA.DbAccess.Repository;
using IVA.DTO;
using IVA.DTO.Contract;
using IVA.FindExpert.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace IVA.FindExpert.Controllers
{
    public class ServiceRequestController : BaseController
    {
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetByBuyerId(long BuyerId)
        {
            var requests = new List<ServiceRequestModel>();
            using (AppDBContext context = new AppDBContext())
            {
                requests = new ServiceRequestRepository(context).GetByBuyerId(BuyerId).
                    OrderByDescending(r => r.TimeOccured).
                    Select(
                    i => new ServiceRequestModel
                    {
                        Id = i.Id,
                        Code = i.Code,
                        InsuranceTypeId = i.InsuranceTypeId,
                        UserId = i.UserId,
                        CreatedDate = i.TimeOccured.GetAdjustedTime().ToString("yyyy-MM-dd"),
                        ClaimType = i.ClaimType,
                        UsageType = i.UsageType,
                        RegistrationCategory = i.RegistrationCategory,
                        VehicleNo = i.VehicleNo,
                        VehicleValue = i.VehicleValue,
                        VehicleYear = i.VehicleYear,
                        IsFinanced = i.IsFinanced,
                        Status = i.Status,
                        ExpiryDate = i.TimeOccured.GetAdjustedTime().AddDays(Constant.DAYS_TO_EXPIRE_REQUEST).ToString("yyyy-MM-dd"),
                        QuotationList = GetServiceQuotations(i.Id)
                    }).ToList();
            }
            return Json(requests);
        }

        [HttpGet]
        [Authorize]
        public IHttpActionResult GetByAgentId(long AgentId)
        {
            var requests = new List<ServiceRequestModel>();
            using (AppDBContext context = new AppDBContext())
            {
                requests = new ServiceRequestRepository(context).GetByAgentId(AgentId).
                    OrderByDescending(r => r.TimeOccured).
                    Select(
                    i => new ServiceRequestModel
                    {
                        Id = i.Id,
                        Code = i.Code,
                        InsuranceTypeId = i.InsuranceTypeId,
                        UserId = i.UserId,
                        CreatedDate = i.TimeOccured.GetAdjustedTime().ToString("yyyy-MM-dd"),
                        ClaimType = i.ClaimType,
                        UsageType = i.UsageType,
                        RegistrationCategory = i.RegistrationCategory,
                        VehicleNo = i.VehicleNo,
                        VehicleValue = i.VehicleValue,
                        VehicleYear = i.VehicleYear,
                        IsFinanced = i.IsFinanced,
                        Status = i.Status,
                        ExpiryDate = i.TimeOccured.GetAdjustedTime().AddDays(Constant.DAYS_TO_EXPIRE_REQUEST).ToString("yyyy-MM-dd"),
                        QuotationList = GetServiceQuotations(i.Id)
                    }).ToList();
            }
            return Json(requests);
        }

        [HttpGet]
        [Authorize]
        public IHttpActionResult GetPendingByAgentId(long AgentId)
        {
            var requests = new List<ServiceRequestModel>();
            using (AppDBContext context = new AppDBContext())
            {
                requests = new ServiceRequestRepository(context).GetByAgentId(AgentId).
                    Where(r => r.Status == (int)Constant.ServiceRequestStatus.PendingResponse).
                    OrderByDescending(r => r.TimeOccured).
                    Select(
                    i => new ServiceRequestModel
                    {
                        Id = i.Id,
                        Code = i.Code,
                        InsuranceTypeId = i.InsuranceTypeId,
                        UserId = i.UserId,
                        CreatedDate = i.TimeOccured.GetAdjustedTime().ToString("yyyy-MM-dd"),
                        ClaimType = i.ClaimType,
                        UsageType = i.UsageType,
                        RegistrationCategory = i.RegistrationCategory,
                        VehicleNo = i.VehicleNo,
                        VehicleValue = i.VehicleValue,
                        VehicleYear = i.VehicleYear,
                        IsFinanced = i.IsFinanced,
                        Status = i.Status,
                        ExpiryDate = i.TimeOccured.GetAdjustedTime().AddDays(Constant.DAYS_TO_EXPIRE_REQUEST).ToString("yyyy-MM-dd"),
                        QuotationList = GetServiceQuotations(i.Id)
                    }).ToList();
            }
            return Json(requests);
        }

        [HttpGet]
        [Authorize]
        public IHttpActionResult GetFollowUpByAgentId(long AgentId)
        {
            var requests = new List<ServiceRequestModel>();
            using (AppDBContext context = new AppDBContext())
            {
                requests = new ServiceRequestRepository(context).GetByAgentId(AgentId).
                    Where(r => r.TimeOccured.AddDays(Constant.DAYS_TO_EXPIRE_REQUEST).AddHours(-Constant.HOURS_TO_FOLLOW_UP) < DateTime.Now.ToUniversalTime()
                        && (r.Status != (int)Constant.ServiceRequestStatus.Expired || r.Status != (int)Constant.ServiceRequestStatus.Closed)).
                    OrderByDescending(r => r.TimeOccured).
                    Select(
                    i => new ServiceRequestModel
                    {
                        Id = i.Id,
                        Code = i.Code,
                        InsuranceTypeId = i.InsuranceTypeId,
                        UserId = i.UserId,
                        CreatedDate = i.TimeOccured.GetAdjustedTime().ToString("yyyy-MM-dd"),
                        ClaimType = i.ClaimType,
                        UsageType = i.UsageType,
                        RegistrationCategory = i.RegistrationCategory,
                        VehicleNo = i.VehicleNo,
                        VehicleValue = i.VehicleValue,
                        VehicleYear = i.VehicleYear,
                        IsFinanced = i.IsFinanced,
                        Status = i.Status,
                        ExpiryDate = i.TimeOccured.GetAdjustedTime().AddDays(Constant.DAYS_TO_EXPIRE_REQUEST).ToString("yyyy-MM-dd"),
                        TimeToExpire = getTimeToExpire(i.TimeOccured),
                        QuotationList = GetServiceQuotations(i.Id)
                    }).ToList();                
            }
            return Json(requests);
        }

        private string getTimeToExpire(DateTime CreatedDate)
        {
            var expDate = CreatedDate.GetAdjustedTime().AddDays(Constant.DAYS_TO_EXPIRE_REQUEST);
            var timeToexpire = DateTime.Now.ToUniversalTime().GetAdjustedTime().Subtract(expDate);
            if (timeToexpire.Hours > 0)
                return timeToexpire.Hours.ToString() + "h " + timeToexpire.Minutes + "m";
            else
                return timeToexpire.Minutes + "mins";
        }

        [HttpGet]
        [Authorize]
        public IHttpActionResult GetById(long Id)
        {
            ServiceRequest request = null;
            ServiceRequestModel model = null;
            IUser buyer = null;
            IUserProfile buyerProfile = null;

            using (AppDBContext context = new AppDBContext())
            {
                request = new ServiceRequestRepository(context).GetById(Id);
                buyer = new UserRepository(context).GetByUserId(request.UserId);
                buyerProfile = new UserProfileRepository(context).GetByUserId(request.UserId);
            }

            if(request != null)
            {
                model = new ServiceRequestModel
                {
                    Id = request.Id,
                    Code = request.Code,
                    InsuranceTypeId = request.InsuranceTypeId,
                    UserId = request.UserId,
                    CreatedDate = request.TimeOccured.GetAdjustedTime().ToString("yyyy-MM-dd"),
                    ClaimType = request.ClaimType,
                    UsageType = request.UsageType,
                    RegistrationCategory = request.RegistrationCategory,
                    VehicleNo = request.VehicleNo,
                    VehicleValue = request.VehicleValue, 
                    VehicleYear = request.VehicleYear,
                    IsFinanced = request.IsFinanced,
                    Status = request.Status,
                    ExpiryDate = request.TimeOccured.GetAdjustedTime().AddDays(Constant.DAYS_TO_EXPIRE_REQUEST).ToString("yyyy-MM-dd"),
                    QuotationList = GetServiceQuotations(request.Id)
                };

                if(buyer != null)
                {
                    model.BuyerName = buyer.Name;
                    model.BuyerMobile = buyer.UserName;
                    model.IsAllowPhone = false;                  

                    if(buyerProfile != null)
                    {
                        model.BuyerName = buyerProfile.FirstName + " " + buyerProfile.LastName;
                        model.BuyerPhone = buyerProfile.Phone;
                        model.City = buyerProfile.City;
                        if (buyerProfile.ContactMethod != (int)Constant.ContactMethod.Message)
                            model.IsAllowPhone = true;
                    }
                }
            }
            return Json(model);
        }

        [HttpPost]
        //[Authorize]
        public IHttpActionResult Save(ServiceRequestModel Model)
        {
            ServiceRequest SR = new ServiceRequest();
            SR.Id = Model.Id;
            SR.InsuranceTypeId = Model.InsuranceTypeId;
            SR.Code = Model.Code;            
            SR.UserId = Model.UserId;
            SR.TimeOccured = DateTime.Now.ToUniversalTime();
            SR.ClaimType = Model.ClaimType;
            SR.UsageType = Model.UsageType;
            SR.RegistrationCategory = Model.RegistrationCategory;
            SR.VehicleNo = Model.VehicleNo;
            SR.VehicleValue = Model.VehicleValue;
            SR.VehicleYear = Model.VehicleYear;
            SR.IsFinanced = Model.IsFinanced;
            //SR.Images = Model.Images;

            using (AppDBContext context = new AppDBContext())
            {
                var repo = new ServiceRequestRepository(context);
                if (SR.Id == 0)
                {
                    SR.Id = repo.Add(SR);
                    AssignRequestToAgents(SR.Id);
                }                    
                else
                    repo.Update(SR);

                if (SR.Images != null)
                {
                    var imageRepo = new VehicleImageRepository(context);
                    imageRepo.Update(SR.Id, SR.Images);
                }                
            }

            return Json(SR);
        }

        private List<RequestQuotationViewModel> GetServiceQuotations(long SRId)
        {
            List<RequestQuotationViewModel> models = null;
            List<RequestQuotation> quotes = null;
            using (AppDBContext context = new AppDBContext())
            {
                quotes = new RequestQuotationRepository(context).GetByRequest(SRId);

                if (quotes != null)
                {                    
                    models = quotes.Select(q => new RequestQuotationViewModel
                    {
                        Id = q.Id,
                        ServiceRequestId = q.ServiceRequestId,
                        QuotationTemplateId = q.QuotationTemplateId,
                        Premimum = q.Premimum?.ToString("0.00"),
                        Cover = q.Cover?.ToString("0.00"),
                        AgentId = q.AgentId,
                        AgentName = q.Agent?.Name,
                        AgentContact = q.Agent?.UserName,
                        QuotationTemplateName = q.QuotationTemplate.Name,
                        Status = q.Status ?? 0,
                        CompanyId = q.Agent.CompanyId ?? 0,
                        CompanyName = q.Agent.Company?.Name

                    }).ToList();
                }
            }
            return models;
        }
        
        private void AssignRequestToAgents(long ServiceRequestId)
        {
            try
            {
                //get all companies
                using (AppDBContext context = new AppDBContext())
                {
                    var companies = new CompanyRepository(context).GetAll();

                    foreach (var company in companies)
                    {
                        //get agents for each company
                        var agents = new UserRepository(context).GetAgentsByCompany(company.Id);

                        if (agents == null)
                            continue;

                        Dictionary<long, int> counts = new Dictionary<long, int>();

                        foreach (var agent in agents)
                        {
                            //get open requests for each agent
                            var agentRequests = new AgentServiceRequestRepository(context).GetByAgentId(agent.Id)?.Where(
                                r => (r.Status ?? 0) != (int)Constant.ServiceRequestStatus.Closed ||
                                    (r.Status ?? 0) != (int)Constant.ServiceRequestStatus.Expired);
                            if (agentRequests != null)
                                counts.Add(agent.Id, agentRequests.Count());
                            else
                                counts.Add(agent.Id, 0);
                        }

                        //find agent with min no of requests
                        //order by id asc and get top 1 agent
                        var item = counts.OrderBy(i => i.Value).ThenBy(i => i.Key).First();

                        //assign the request to agent
                        AgentServiceRequest asr = new AgentServiceRequest
                        {
                            AgentId = item.Key,
                            ServiceRequestId = ServiceRequestId,
                            Status = (int)Constant.ServiceRequestStatus.PendingResponse,
                            CreatedTime = DateTime.Now.ToUniversalTime()
                        };

                        new AgentServiceRequestRepository(context).Add(asr);
                    }
                }
            }
            catch(Exception e)
            {

            }

        }
    }

}