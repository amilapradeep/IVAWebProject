﻿using Core.Log;
using IVA.DbAccess;
using IVA.DbAccess.Repository;
using IVA.DTO;
using IVA.DTO.Contract;
using IVA.FindExpert.Helpers.ActionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IVA.FindExpert.Controllers
{
    public class UserController : BaseController
    {
        [HttpPost]
        [Authorize]
        [AccessActionFilter]
        public IHttpActionResult UpdateProfile(UserProfileModel model)
        {
            try
            {

                if (model != null)
                {
                    UserProfile profile = new UserProfile
                    {
                        UserId = model.UserId,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        Email = model.Email,
                        Phone = model.Phone,
                        Mobile = model.Mobile,
                        Street = model.Street,
                        City = model.City,
                        Image = model.Image,
                        Location = model.Location,
                        LocationLatitude = model.LocationLatitude,
                        LocationLongitude = model.LocationLongitude,
                        ContactMethod = model.ContactMethod,
                        BankId = model.BankId,
                        BankBranch = model.BankBranch,
                        AccountName = model.AccountName,
                        AccountNo = model.AccountNo,
                        NotificationFrequencyMinutes = model.NotificationFrequencyMinutes
                    };

                    using (AppDBContext context = new AppDBContext())
                    {
                        var repo = new UserProfileRepository(context);
                        var existing = repo.GetByUserId(profile.UserId);
                        if (existing == null)
                            repo.Add(profile);
                        else
                            repo.Update(profile);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(UserController), ex.Message + ex.StackTrace, LogType.ERROR);
                return InternalServerError();
            }
        }

        [HttpGet]
        [Authorize]
        [AccessActionFilter]
        public IHttpActionResult GetProfileByUserId(long UserId)
        {
            IUserProfile profile = null;
            UserProfileModel model = null;
            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    profile = new UserProfileRepository(context).GetByUserId(UserId);
                }
                if (profile != null)
                {
                    model = new UserProfileModel
                    {
                        Id = profile.Id,
                        UserId = profile.UserId,
                        FirstName = profile.FirstName,
                        LastName = profile.LastName,
                        Gender = profile.Gender ?? 0,
                        Email = profile.Email,
                        Phone = profile.Phone,
                        Mobile = profile.Mobile,
                        Street = profile.Street,
                        City = profile.City,
                        Image = profile.Image,
                        Location = profile.Location,
                        ContactMethod = profile.ContactMethod ?? 0,
                        BankId = profile.BankId,
                        BankBranch = profile.BankBranch,
                        AccountName = profile.AccountName,
                        AccountNo = profile.AccountNo,
                        NotificationFrequencyMinutes = profile.NotificationFrequencyMinutes ?? 0
                    };
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(UserController), ex.Message + ex.StackTrace, LogType.ERROR);
                return InternalServerError();
            }
        }

        [HttpPost]
        [Authorize]
        [AccessActionFilter]
        public IHttpActionResult FeedBack(UserFeedbackModel Model)
        {
            var feedback = new UserFeedback();
            feedback.Date = DateTime.Now.ToUniversalTime();
            feedback.Description = Model.Description;
            feedback.Rating = 0;
            feedback.UserId = Model.UserId;

            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    try
                    {
                        var repo = new UserFeedbackRepository(context);
                        repo.Add(feedback);
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(UserController), ex.Message + ex.StackTrace, LogType.ERROR);
                return InternalServerError();
            }
        }

        [HttpPost]
        [Authorize]
        [AccessActionFilter]
        public IHttpActionResult AddUserDevice(UserDeviceModel Model)
        {
            long ret = 0;
            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    ret = new UserDeviceRepository(context).Add(
                        new UserDevice
                        {
                            UserId = Model.UserId,
                            DeviceToken = Model.DeviceToken,
                            DeviceId = Model.DeviceId
                        });
                }
                return Ok(ret);
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(UserController), ex.Message + ex.StackTrace, LogType.ERROR);
                return InternalServerError();
            }
        }
    }
}
