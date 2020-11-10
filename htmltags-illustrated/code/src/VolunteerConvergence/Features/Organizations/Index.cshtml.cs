using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.Features.Organizations
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public Result Data { get; set; }

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync()
        {
            var result = await _mediator.Send(new Query());
            Data = result;
        }

        public class Query : IRequest<Result> { }

        public class Result
        {
            public List<OrganizationDto> Organizations{ get; set; } = new List<OrganizationDto>();

            public class OrganizationDto
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public string OrganizationTypeName { get; set; }
                public string Description { get; set; }
                public NeedLevel NeedLevel { get; set; }                    
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Organization, Result.OrganizationDto>();
        }

        public class QueryHandler : IRequestHandler<Query, Result>
        {
            private readonly VolConContext _db;
            private readonly IMapper _mapper;

            public QueryHandler(VolConContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                // For sample purposes only.
                // In production, please don't load an entire table that you don't control the size of.
                var orgs = await _db.Organizations
                    .Include(o => o.OrganizationType)
                    .ToArrayAsync(cancellationToken);

                var organizationDtos = _mapper.Map<Result.OrganizationDto[]>(orgs);

                var result = new Result();
                result.Organizations.AddRange(organizationDtos);
                return result;
            }
        }
    }
}
