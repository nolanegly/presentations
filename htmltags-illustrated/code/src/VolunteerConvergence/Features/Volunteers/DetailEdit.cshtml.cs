using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VolunteerConvergence.Models;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace VolunteerConvergence.Features.Volunteers
{
    public class DetailEditModel : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public Command Data { get; set; }


        public DetailEditModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync(Query query) => Data = await _mediator.Send(query);

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);
            
            return this.RedirectToPageAndIdJson("Detail", Data.VolunteerId.Value);
        }

        public class Query : IRequest<Command>
        {
            public Guid? Id { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(m => m.Id).NotNull();
            }
        }

        public class Command : IRequest
        {
            public Guid? Id  { get; set; }
            public byte[] RowVersion { get; set; }
            public Guid? VolunteerId { get; set; }
            public Organization Organization { get; set; }
            public int? NumHours { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotNull();
                RuleFor(c => c.VolunteerId).NotNull();
                RuleFor(c => c.Organization).NotNull();
                RuleFor(c => c.NumHours).NotNull().GreaterThan(0);
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<VolunteerHour, Command>()
                .ReverseMap();
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly VolConContext _db;
            private readonly IMapper _mapper;

            public QueryHandler(VolConContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
            {
                var volunteerHour = await _db.VolunteerHours
                    .Include(vh => vh.Organization)
                    .SingleOrDefaultAsync(vh => vh.Id.Equals(request.Id.Value), cancellationToken);

                var result = _mapper.Map<Command>(volunteerHour);
                return result;
            }
        }

        public class CommandHandler : IRequestHandler<Command>
        {
            private readonly VolConContext _db;
            private readonly IMapper _mapper;

            public CommandHandler(VolConContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<MediatR.Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var volunteerHour = await _db.VolunteerHours
                    .SingleOrDefaultAsync(vh => vh.Id.Equals(command.Id.Value), cancellationToken);

                if (volunteerHour == null)
                    throw new ValidationException("Could not find the specified volunteer hour entry");

                _mapper.Map(command, volunteerHour);
                _db.OverrideOriginalConcurrencyToken(volunteerHour, command.RowVersion);

                return default; // same as return Unit.Value;
            }
        }
    }
}
