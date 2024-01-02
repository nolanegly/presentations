using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using VolunteerConvergence.Features.Applicants;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.IntegrationTests.Features.Applicants
{
    using static SliceFixture;

    class IndexTests
    {
        public async Task Should_load_all_applicants()
        {
            // arrange
            var applicantOne = new ApplicantBuilder().Build("Charlie", "Brown");
            var applicantTwo = new ApplicantBuilder().Build("Sally", "Brown");

            await InsertAsync(applicantOne, applicantTwo);

            // act
            var result = await SendAsync(new Query());

            // assert
            AssertContains(result, applicantOne);
            AssertContains(result, applicantTwo);
        }

        private void AssertContains(Result result, Applicant expected)
        {
            var actual = result.Applicants.Single(a =>
                a.Email.Equals(expected.Email, StringComparison.InvariantCultureIgnoreCase));

            actual.FirstName.ShouldBe(expected.FirstName);
            actual.LastName.ShouldBe(expected.LastName);
        }
    }
}
