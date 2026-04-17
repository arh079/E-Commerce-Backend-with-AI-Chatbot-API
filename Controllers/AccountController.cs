using ChatAPI.DTO;
using ChatAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace ChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            config = configuration;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO UserFromRequest)
        {
            //Save DB
            if (ModelState.IsValid)
            {
                ApplicationUser User = new ApplicationUser();
                User.UserName = UserFromRequest.UserName;
                User.Email = UserFromRequest.Email;
                IdentityResult result =
                await userManager.CreateAsync(User, UserFromRequest.Password);    //Hash Pass
                if (result.Succeeded)
                {
                    return Ok("Created");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }
            }


            return BadRequest(ModelState);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO UserFromRequest)
        {
            if (ModelState.IsValid)
            {
                //check
                ApplicationUser User =
                    await userManager.FindByEmailAsync(UserFromRequest.UserEmail);
                if (User != null)
                {
                    bool Check =
                    await userManager.CheckPasswordAsync(User, UserFromRequest.Password);
                    if (Check == true)
                    {
                        //generate token
                        List<Claim> UserClaims = new List<Claim>();
                        UserClaims.Add(new Claim(ClaimTypes.NameIdentifier, User.Id));
                        UserClaims.Add(new Claim(ClaimTypes.Name, User.UserName));
                        //UserClaims.Add(new Claim(ClaimTypes.Email, User.Email));


                        //Token Generated id change (JWT Predefined Claims)
                        UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                        var UserRoles = await userManager.GetRolesAsync(User);
                        foreach (var role in UserRoles)
                        {
                            UserClaims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        var SigninKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecritKey"]));

                        SigningCredentials signingCredentials =
                            new SigningCredentials(SigninKey, SecurityAlgorithms.HmacSha256);


                        //design token
                        JwtSecurityToken mytoken = new JwtSecurityToken(

                            audience: config["JWT:AudienceIP"],
                            issuer: config["JWT:IssuerIP"],
                            expires: DateTime.Now.AddHours(1),
                            claims: UserClaims,
                            signingCredentials: signingCredentials
                            );


                        //generate token response
                        return Ok(new

                        {
                            token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = DateTime.Now.AddHours(1) //mytoken.ValidTo
                        }
                            );
                    }
                }
                ModelState.AddModelError("UserName", "UserName OR Password Invalid");

            }
            return BadRequest(ModelState);
        }

        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();


            user.UserName = string.IsNullOrWhiteSpace(dto.UserName) ? user.UserName : dto.UserName;
            user.Email = string.IsNullOrWhiteSpace(dto.UserEmail) ? user.Email : dto.UserEmail;

            await userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(dto.currPass) && !string.IsNullOrEmpty(dto.newPass))
            {
                var result = await userManager.ChangePasswordAsync(user, dto.currPass, dto.newPass);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);
            }

            return Ok("User updated successfully"); 
        }


        [Authorize]
        [HttpGet("GetUserId")]
        public IActionResult GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            return Ok(userId);
        }
    } 
}
