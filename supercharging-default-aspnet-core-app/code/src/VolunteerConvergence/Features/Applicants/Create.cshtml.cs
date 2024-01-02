using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VolunteerConvergence.Models;
using ValidationException = FluentValidation.ValidationException;

namespace VolunteerConvergence.Features.Applicants
{
    public class CreateModel : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public Command Data { get; set; }


        public CreateModel(IMediator mediator)
        {
            _mediator = mediator;
        }


        public async Task<IActionResult> OnPostFullAsync()
        {
            await _mediator.Send(Data);

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostAjaxAsync()
        {
            await _mediator.Send(Data);

            return this.RedirectToPageJson("Index");
        }

        public class Command : IRequest<Guid>
        {
            [Required, MaxLength(500)]
            public string FirstName { get; set; }

            [Required, MaxLength(500)]
            public string LastName { get; set; }

            public string Email { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Email).NotEmpty().MaximumLength(500);

                RuleFor(c => c.Email)
                    .Must((cmd, email) => ! (
                        email.Contains(cmd.FirstName, StringComparison.InvariantCultureIgnoreCase) ||
                        email.Contains(cmd.LastName, StringComparison.InvariantCultureIgnoreCase)))
                    .When(c => c.Email != null &&  (c.FirstName?.Contains("nolan", StringComparison.InvariantCultureIgnoreCase) ?? false))
                    .WithMessage("Nolan, don't register with your personal email address");
            }
        }

        public class Profile : MappingProfile
        {
            public Profile() => CreateMap<Command, Applicant>();
        }

        public class CommandHandler : IRequestHandler<Command, Guid>
        {
            private readonly VolConContext _db;
            private readonly IMapper _mapper;

            public CommandHandler(VolConContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
            {
                var applicant = _mapper.Map<Applicant>(request);

                if (request.Email.EndsWith("AlreadyInUse.com", StringComparison.InvariantCultureIgnoreCase))
                    throw new ValidationException("The email address is already in use");

                await _db.Applicants.AddAsync(applicant, cancellationToken);

                return applicant.Id;
            }
        }
    }
}
