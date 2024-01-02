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

namespace VolunteerConvergence.Features.Volunteers
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
            public List<VolunteerDto> Volunteers { get; set; } = new List<VolunteerDto>();

            public class VolunteerDto
            {
                public Guid Id { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
                public string Email { get; set; }
                public DateTime JoinDate { get; set; }
                public string JoinDateDisplay => JoinDate.ToShortDateString();
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Volunteer, Result.VolunteerDto>();
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
                var volunteers = await _db.Volunteers.ToArrayAsync(cancellationToken);

                var volunteerDtos = _mapper.Map<Result.VolunteerDto[]>(volunteers);

                var result = new Result();
                result.Volunteers.AddRange(volunteerDtos);
                return result;
            }
        }
    }
}
