using Microsoft.EntityFrameworkCore;
using Task_Management_System.Configurations;
using Task_Management_System.Data;
using Task_Management_System.DTOs.OrganizationDto;
using Task_Management_System.DTOs.RequestDto;
using Task_Management_System.Enums;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.OrganizationUserService
{
    public class OrganizationUserService : IOrganizationUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserConfig _userConfig;

        public string AdminRole = ApplicationRoleNames.OrganizationAdministrator;
        public const string MemberRole = ApplicationRoleNames.OrganizationMember;

        public OrganizationUserService(ApplicationDbContext context, IUserConfig userConfig)
        {
            _context = context;
            _userConfig = userConfig;
        }

        public async Task<string> AcceptOrganizationRequest(Guid organizationId, string userEmail)
        {
            try
            {
                var user = await _userConfig.GetUser(userEmail);

                var retrieveOrg = await _context.Organizations.FindAsync(organizationId);

                if (retrieveOrg == null)
                {
                    throw new Exception("Organization does not exist");
                }

                var retriveRequest = await _context.Requests.Where(req => req.OrganizationId == organizationId && req.UserId == user.Id).FirstOrDefaultAsync();

                if (retriveRequest == null)
                {
                    throw new Exception("No such request");
                }
                else
                {
                    var retrieveRole = await _userConfig.GetRole(MemberRole);
                    var isInRole = await _userConfig.UserInRole(MemberRole, user);
                    OrganizationUser requestToModel;

                    if (isInRole == true)
                    {
                        requestToModel = new OrganizationUser
                        {
                            Id = Guid.NewGuid(),
                            User = user,
                            Organization = retrieveOrg,
                            Role = retrieveRole,
                        };
                        await _context.OrganizationUser.AddAsync(requestToModel);
                    }
                    else
                    {
                        var addToRole = await _userConfig.AddUserToRole(MemberRole, user);
                        if (addToRole.Succeeded)
                        {
                            requestToModel = new OrganizationUser
                            {
                                Id = Guid.NewGuid(),
                                User = user,
                                Organization = retrieveOrg,
                                Role = retrieveRole,
                            };
                            await _context.OrganizationUser.AddAsync(requestToModel);
                        }

                        else
                        {
                            throw new Exception("Unable to add user to role" + MemberRole);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return " User joined the organization successfully";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //function name should be send request to user
        public async Task<string> SendOrganizationRequest(Guid organizationId, string inviteeEmail, string adminId)
        {
            try
            {
                var adminUser = await _userConfig.ValidateOrganizationUser(adminId, organizationId, AdminRole);

                if (adminUser == null)
                {
                    throw new Exception("Unable to validate user");
                }
                else
                {
                    var retrieveInvitee = await _userConfig.GetUser(inviteeEmail);

                    var checkIfUserOrgExist = await _context.OrganizationUser
                        .Where(u => u.User == retrieveInvitee && u.Organization.Id == organizationId)
                        .SingleOrDefaultAsync();

                    if (retrieveInvitee == null || retrieveInvitee.Email is null && checkIfUserOrgExist != null)
                    {
                        throw new Exception("This user does not exist in our system or you have user in the organization");
                    }

                    else
                    {
                        var request = await _context.Requests
                            .SingleOrDefaultAsync(req => req.UserId == retrieveInvitee.Id && req.OrganizationId == organizationId);

                        if (request != null)
                        {
                            throw new Exception("There is an existing request for this user for this organization");
                        }

                        var sendResponse = await _context.Requests.AddAsync(new Requests
                        {
                            Id = Guid.NewGuid(),
                            OrganizationId = organizationId,
                            InviteeEmail = retrieveInvitee.Email,
                            UserId = retrieveInvitee.Id,
                            User = retrieveInvitee,
                            Status = Status.Pending
                        });
                    }
                    await _context.SaveChangesAsync();
                    return "Request sent to the user";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> RevokeOrganizationRequest(Guid organizationId, string receiver, string adminId)
        {
            try
            {
                var adminUser = await _userConfig.ValidateOrganizationUser(adminId, organizationId, AdminRole);

                if (adminUser == null)
                {
                    throw new Exception("Unable to validate user");
                }
                else
                {
                    var retrieveInvitee = await _userConfig.GetUser(receiver);

                    var checkIfOrgExist = await _context.Organizations
                        .Where(org => org.Id == organizationId)
                        .SingleOrDefaultAsync();

                    if (checkIfOrgExist == null)
                    {
                        throw new Exception("Organition does not exist");
                    }

                    var sentRequest = await _context.Requests
                            .SingleOrDefaultAsync(req => req.UserId == retrieveInvitee.Id && req.OrganizationId == organizationId);

                    if (sentRequest == null)
                    {
                        throw new Exception("Request does not exist");
                    }

                    _context.Requests.Remove(sentRequest);
                    await _context.SaveChangesAsync();

                    return "Removed Successfully";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GetOrganizationDto> GetOrganization(Guid organizationId, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var getSpecificOrganization = await _context.OrganizationUser
                .Include(role => role.Role)
                .Include(org => org.Organization)
                .Include(user => user.User)
                .Where(org => org.Organization.Id == organizationId && org.User == user)
                .SingleOrDefaultAsync();

            if (getSpecificOrganization == null)
            {
                throw new Exception("You don't belong to this organization");
            }
            else
            {
                var getOrg = new GetOrganizationDto
                {
                    Id = organizationId,
                    Name = getSpecificOrganization.Organization.Name,
                    Creator = getSpecificOrganization.User.UserName,
                    Description = getSpecificOrganization.Organization.Description,
                    Role = getSpecificOrganization.Role.Name,
                    DateCreated = getSpecificOrganization.Organization.CreatedTime,
                    DateJoined = getSpecificOrganization.Organization.DateJoined
                };

                return getOrg;
            }
        }

        //this service helps retrieve organization user either belongs to or created by them
        public async Task<IEnumerable<GetOrganizationDto>> GetOrganizations(OrganizationFilter? filter, string mail)
        {
            var user = await _userConfig.GetUser(mail);

            IQueryable<OrganizationUser> query = _context.OrganizationUser
                .Where(u => u.User.Id == user.Id)
                .Include(org => org.Organization)
                .Include(role => role.Role)
                .Include(user => user.User);

            if (filter.HasValue)
            {
                if (filter.Value == OrganizationFilter.Owned)
                {
                    query = query.Where(val => val.Organization.CreatedBy == user.Id);
                }
                else
                {
                    query = query.Where(val => val.Organization.CreatedBy != user.Id);
                }
            }
            var organizationList = await query.ToListAsync();

            if (organizationList.Count < 1)
            {
                return new List<GetOrganizationDto>();
            }

            var creatorIds = organizationList
                .Select(o => o.Organization.CreatedBy).Distinct().ToList();

            var creators = await _context.Users
                .Where(u => creatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => new { u.Email, u.FirstName });


            var mapOrg = organizationList.Select(check => new GetOrganizationDto
            {
                Id = check.Organization.Id,
                Name = check.Organization.Name,
                Description = check.Organization.Description,
                Role = check.Role.Name,
                Creator = creators[check.Organization.CreatedBy].Email == user.Email ? "owned" : "Joined",
                DateCreated = check.Organization.CreatedTime,
                DateJoined = check.Organization.DateJoined,
            }).ToList();

            return mapOrg;
        }

        public async Task<IEnumerable<OrganizationUserDto>> OrganizationUsers(Guid organizationId, string userEmail)
        {
            var user = await _userConfig.GetUser(userEmail);
            var userExistInOrg = await _context.OrganizationUser
                .SingleOrDefaultAsync(usr => usr.User.Email == user.Email && usr.Organization.Id == organizationId);

            if (user == null || userExistInOrg == null)
            {
                throw new Exception("User doesn't exist");
            }
            else
            {
                var usersInOrg = await _context.OrganizationUser
                    .Include(users => users.User).Include(role => role.Role)
                    .Where(org => org.Organization.Id == organizationId).ToListAsync();

                if (usersInOrg is null)
                {
                    return new List<OrganizationUserDto>();
                }

                var users = usersInOrg.Select(orgUsers => new OrganizationUserDto
                {
                    UserId = orgUsers.User.Id,
                    UserName = orgUsers.User.FirstName + " " + orgUsers.User.LastName,
                    UserEmail = orgUsers.User.Email!,
                    UserRole = orgUsers.Role?.Name!,
                }).ToList();

                return users;
            }
        }

        public async Task<string> RejectOrganizationRequest(Guid organizationId, string userEmail)
        {
            try
            {
                var user = await _userConfig.GetUser(userEmail);

                var retrieveOrg = await _context.Organizations.FindAsync(organizationId);

                if (retrieveOrg == null)
                {
                    throw new Exception("Organization does not exist");
                }

                var retrieveRole = await _userConfig.GetRole(userEmail);

                var retriveRequest = await _context.Requests
                    .Where(req => req.OrganizationId == organizationId && req.UserId == user.Id)
                    .SingleOrDefaultAsync();

                if (retriveRequest == null)
                {
                    throw new Exception("No such request");
                }

                _context.Requests.Remove(retriveRequest);

                await _context.SaveChangesAsync();

                return "Successfully rejected";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> RemoveUserFromOrganization(Guid organizationId, string memberMail, string adminId)
        {
            try
            {
                var adminUser = await _userConfig.ValidateOrganizationUser(adminId, organizationId, AdminRole);

                if (adminUser == null)
                {
                    throw new Exception("Unable to validate user");
                }
                else
                {
                    var member = await _userConfig.GetUser(memberMail);

                    var checkIfUserOrgExist = await _context.OrganizationUser
                        .Where(u => u.User == member && u.Organization.Id == organizationId && u.Organization.CreatedBy != adminUser.Id)
                        .SingleOrDefaultAsync();

                    if (member == null || checkIfUserOrgExist == null)
                    {
                        throw new Exception("This user does not exist in the system");
                    }

                    else
                    {
                        _context.OrganizationUser.Remove(checkIfUserOrgExist);
                    }
                    await _context.SaveChangesAsync();
                    return "user removed from the organization";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<InvitationRequestDto>> InvitationList(string mail)
        {
            var user = await _userConfig.GetUser(mail);

            var response = await (from req in _context.Requests
                                  join org in _context.Organizations
                                  on req.OrganizationId equals org.Id
                                  where req.UserId == user.Id
                                  select new InvitationRequestDto
                                  {
                                      OrganizationId = req.OrganizationId,
                                      OrganizationName = org.Name
                                  }).ToListAsync();

            return response;
        }

        public async Task<IEnumerable<SentRequestDto>> SentRequests(Guid organizationId)
        {
            var retrieveOrganization = await _context.Organizations.FindAsync(organizationId);

            if (retrieveOrganization == null)
            {
                throw new Exception("Organization does not exist");
            }
            var retrieveList = await _context.Requests.Include(user => user.User).Where(req => req.OrganizationId == organizationId && req.Status == Status.Pending).ToListAsync();

            if (retrieveList == null)
            {
                return Enumerable.Empty<SentRequestDto>();
            }

            var response = retrieveList.Select(req => new SentRequestDto
            {
                Id = req.Id,
                Name = req.User.FirstName,
                Email = req.InviteeEmail,
            }).ToList();

            return response;
        }

        public async Task<OrganizationResponse> GetPaginatedOrganizations(OrganizationFilter? filter, int? page, int itemPerPage, string userMail)
        {
            var user = await _userConfig.GetUser(userMail);

            int defaultItemsPerPage = 10;

            var items = itemPerPage == 0 ? defaultItemsPerPage : itemPerPage;
            int currentPage = page.HasValue && page > 0 ? page.Value : 1;

            var query = _context.OrganizationUser
                .Include(user => user.Role)
                .Include(user => user.Organization)
                .Where(org => org.User.Id == user.Id);

            if (filter != null)
            {
                query = query.Where(org => org.Organization.Filter == filter);
            }

            int totalItems = await query.CountAsync();

            if (totalItems == 0)
            {
                return new OrganizationResponse(new List<GetOrganizationDto>(), 0, 0, 0, 0, 0, 0);
            }

            int pageCount = (int)Math.Ceiling((double)totalItems / items);
            int itemStart = (currentPage - 1) * items + 1;
            int itemEnd = Math.Min(currentPage * items, totalItems);

            var organizationList = await query
                .Skip((currentPage - 1) * items)
                .Take(items)
                .ToListAsync();

            var creatorIds = organizationList.Select(o => o.Organization.CreatedBy).Distinct().ToList();

            var creators = await _context.Users
                .Where(u => creatorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => new { u.Email });
            var mappedOrganizations = organizationList.Select(org => new GetOrganizationDto
            {
                Id = org.Organization.Id,
                Name = org.Organization.Name,
                Description = org.Organization.Description,
                Role = org.Role.Name,
                Creator = creators[org.Organization.CreatedBy].Email == user.Email ? "owned" : "joined"
            }).ToList();

            return new OrganizationResponse(mappedOrganizations, currentPage, totalItems, pageCount, itemStart, itemEnd, organizationList.Count);
        }

        public async Task<OrganizationUserDto> RetrieveOrganizationUser(Guid organizationId, string userEmail)
        {
            var user = await _userConfig.GetUser(userEmail);

            var retrieveOrganization = await _context.Organizations.Where(org => org.Id == organizationId).SingleOrDefaultAsync();

            if (retrieveOrganization == null)
            {
                return new OrganizationUserDto();
            }
            else
            {
                var retrieveOrgUser = await _context.OrganizationUser
                    .Include(u => u.User).Include(o => o.Organization)
                    .Where(ou => ou.User == user && ou.Organization == retrieveOrganization)
                    .SingleOrDefaultAsync();

                if (retrieveOrganization == null)
                {
                    throw new Exception("user doesn't exist in this organization");
                }

                var data = new OrganizationUserDto
                {
                    UserEmail = userEmail,
                    UserId = retrieveOrgUser.User.Id,
                    UserRole = retrieveOrgUser.Role.Name
                };

                return data;
            }
        }
    }
}
