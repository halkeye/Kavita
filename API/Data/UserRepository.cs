﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Constants;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(DataContext context, IMapper mapper, UserManager<AppUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public void Delete(AppUser user)
        {
            _context.AppUser.Remove(user);
        }

        /// <summary>
        /// Gets an AppUser by username. Returns back Progress information.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Progresses)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetAdminUsersAsync()
        {
            return await _userManager.GetUsersInRoleAsync(PolicyConstants.AdminRole);
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await _context.Users
                .Include(x => x.Libraries)
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new MemberDto
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Created = u.Created,
                    LastActive = u.LastActive,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList(),
                    Libraries =  u.Libraries.Select(l => new LibraryDto
                    {
                        Name = l.Name,
                        CoverImage = l.CoverImage,
                        Type = l.Type,
                        Folders = l.Folders.Select(x => x.Path).ToList()
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();
        }

        // public async Task<MemberDto> GetMemberAsync(string username)
        // {
        //     return await _context.Users.Where(x => x.UserName == username)
        //         .Include(x => x.Libraries)
        //         .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        //         .SingleOrDefaultAsync();
        // }
    }
}