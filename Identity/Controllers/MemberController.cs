﻿using Identity.Models;
using Identity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using Identity.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Identity.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        public MemberController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
            : base(userManager, signInManager)
        {
        }

        public IActionResult Index()
        {
            AppUser user = CurrentUser; 
            UserVm userVm = user.Adapt<UserVm>();
            return View(userVm);
        }

        [HttpGet]
        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser;
            UserVm userVm = user.Adapt<UserVm>();
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            return View(userVm);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserVm userVm, IFormFile userPicture)
        {
            ModelState.Remove("Password");
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;
                if (userPicture != null && userPicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);
                        user.Picture = "/UserPicture/" + fileName;
                    }
                }
                user.UserName = userVm.UserName;
                user.Email = userVm.Email;
                user.PhoneNumber = userVm.PhoneNumber;
                user.City = userVm.City;
                user.Gender = (int)userVm.Gender;
                user.BirthDay = userVm.BirthDay;

                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    ViewBag.Success = "true";
                    await _userManager.UpdateSecurityStampAsync(user);
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, true);
                }
                else
                {
                    AddModelError(result);
                }
            }
            return View(userVm);
        }

        [HttpGet]
        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PasswordChange(PasswordChangeVm passwordVm)
        {
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                bool isExist = _userManager.CheckPasswordAsync(user, passwordVm.PasswordOld).Result;
                if (isExist)
                {
                    IdentityResult result = _userManager.ChangePasswordAsync(user, passwordVm.PasswordOld, passwordVm.PasswordNew).Result;
                    if (result.Succeeded)
                    {
                        _userManager.UpdateSecurityStampAsync(user);
                        _signInManager.SignOutAsync();
                        _signInManager.PasswordSignInAsync(user, passwordVm.PasswordNew, true, false);

                        ViewBag.Success = "true";
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Эски сыр туура эмес");
                }
            }
            return View(passwordVm);
        }
    }
}