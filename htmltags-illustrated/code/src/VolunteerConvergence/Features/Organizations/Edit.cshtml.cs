using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VolunteerConvergence.Infrastructure.Tags;
using VolunteerConvergence.Models;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace VolunteerConvergence.Features.Organizations
{
    public class EditModel : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public Command Data { get; set; }


        public EditModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync(Query query) => Data = await _mediator.Send(query);

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);
            
            return this.RedirectToPageJson("Index");
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
            public string Name { get; set; }
            public OrganizationType OrganizationType { get; set; }
            public string Description { get; set; }
            [ExcludeFromOptions((int)Models.NeedLevel.Low)]
            public NeedLevel? NeedLevel { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).NotNull();
                RuleFor(c => c.Name).NotNull();
                RuleFor(c => c.OrganizationType).NotNull();
                RuleFor(c => c.NeedLevel).NotNull();
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Organization, Command>()
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
                var org = await _db.Organizations
                    .Include(o => o.OrganizationType)
                    .SingleOrDefaultAsync(o => o.Id.Equals(request.Id.Value), cancellationToken);

                var result = _mapper.Map<Command>(org);
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
                var org = await _db.Organizations
                    .SingleOrDefaultAsync(o => o.Id.Equals(command.Id.Value), cancellationToken);

                if (org == null)
                    throw new ValidationException("Could not find the specified volunteer hour entry");

                _mapper.Map(command, org);
                _db.OverrideOriginalConcurrencyToken(org, command.RowVersion);

                return default; // same as return Unit.Value;
            }
        }
    }
}
