using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.Features.Volunteers
{
    public class DetailModel : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public Result Data { get; set; }

        public DetailModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync(Query query) => Data = await _mediator.Send(query);

        public class Query : IRequest<Result>
        {
            [Required]
            public Guid? Id { get; set; }
        }

        public class Result
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public DateTime JoinDate { get; set; }

            public List<VolunteerHourDto> VolunteerHours { get; set; } = new List<VolunteerHourDto>();

            public class VolunteerHourDto
            {
                public Guid Id { get; set; }
                public string OrganizationName { get; set; }
                public string OrganizationOrganizationTypeName { get; set; }
                public int NumHours { get; set; }
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Volunteer, Result>();
                CreateMap<VolunteerHour, Result.VolunteerHourDto>()
                    .ForMember(d => d.OrganizationOrganizationTypeName, opt => opt.MapFrom(vh => vh.Organization.OrganizationType.Name));
            } 
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            private readonly VolConContext _db;
            private readonly IMapper _mapper;
            private readonly IConfigurationProvider _mapConfig;

            public QueryHandler(VolConContext db, IMapper mapper, IConfigurationProvider mapConfig)
            {
                _db = db;
                _mapper = mapper;
                _mapConfig = mapConfig;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                Result result = null;
                result = await QueryWithAutoMappingButNoProjection(request, cancellationToken);
                //result = await QueryWithManualMappingAndProjection(request, cancellationToken);
                //result = await QueryWithAutoMappingButFailedAttemptToProject(request, cancellationToken);
                result = await QueryWithAutoMappingAndProjection(request, cancellationToken);

                return result;
            }

            private async Task<Result> QueryWithAutoMappingButNoProjection(Query request, CancellationToken cancellationToken)
            {
                var volunteer = await _db.Volunteers
                    .Include(v => v.VolunteerHours)
                        .ThenInclude(vh => vh.Organization)
                        .ThenInclude(o => o.OrganizationType)
                    .SingleOrDefaultAsync(v => v.Id == request.Id.Value, cancellationToken);

                if (volunteer == null)
                    throw new Exception("Volunteer not found");

                var result = _mapper.Map<Result>(volunteer);
                return result;
            }

            private async Task<Result> QueryWithManualMappingAndProjection(Query request, CancellationToken cancellationToken)
            {
                var result = await _db.Volunteers
                    .Include(v => v.VolunteerHours)
                        .ThenInclude(vh => vh.Organization)
                        .ThenInclude(o => o.OrganizationType)
                    .Where(v => v.Id == request.Id.Value)
                    // IQueryable translates all of this Select into SQL, limiting the number of columns that get projected back to us
                    .Select(v => new Result
                    {
                        FirstName = v.FirstName,
                        LastName = v.LastName,
                        Email = v.Email,
                        JoinDate = v.JoinDate,
                        VolunteerHours = v.VolunteerHours.Select(vh =>
                            new Result.VolunteerHourDto
                            {
                                Id = vh.Id,
                                OrganizationName = vh.Organization.Name,
                                OrganizationOrganizationTypeName = vh.Organization.OrganizationType.Name,
                                NumHours = vh.NumHours
                            }).ToList()
                    })
                    .SingleOrDefaultAsync(cancellationToken);

                if (result == null)
                    throw new Exception("Volunteer not found");

                return result;
            }

            private async Task<Result> QueryWithAutoMappingButFailedAttemptToProject(Query request, CancellationToken cancellationToken)
            {
                var volunteer = await _db.Volunteers
                    .Include(v => v.VolunteerHours)
                        .ThenInclude(vh => vh.Organization)
                        .ThenInclude(o => o.OrganizationType)
                    .Where(v => v.Id == request.Id.Value)
                    // calling AutoMapper (or any expression IQueryable can't translate into SQL) transfers evaluation "client side" to our code instead of "server side" on the database
                    .Select(v => _mapper.Map<Result>(v)) 
                    .SingleOrDefaultAsync(cancellationToken);

                if (volunteer == null)
                    throw new Exception("Volunteer not found");

                var result = _mapper.Map<Result>(volunteer);
                return result;
            }

            private async Task<Result> QueryWithAutoMappingAndProjection(Query request, CancellationToken cancellationToken)
            {
                var result = await _db.Volunteers
                        .Include(v => v.VolunteerHours)
                            .ThenInclude(vh => vh.Organization)
                            .ThenInclude(o => o.OrganizationType)
                        .Where(v => v.Id == request.Id.Value)
                        .ProjectTo<Result>(_mapConfig) // this invokes AutoMapper extensions that do the IQueryable selections for us automatically based on our map configurations
                        .SingleOrDefaultAsync(cancellationToken);

                if (result == null)
                    throw new Exception("Volunteer not found");

                return result;
            }
        }
    }
}
