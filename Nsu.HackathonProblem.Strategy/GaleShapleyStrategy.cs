using System;
using System.Collections.Generic;
using System.Linq;
using Nsu.HackathonProblem.Contracts;

namespace Nsu.HackathonProblem.Strategy
{
    public class GaleShapleyStrategy : ITeamBuildingStrategy
    {
        public IEnumerable<Team> BuildTeams(
            IEnumerable<Employee> teamLeads,
            IEnumerable<Employee> juniors,
            IEnumerable<Wishlist> teamLeadsWishlists,
            IEnumerable<Wishlist> juniorsWishlists)
        {
            var teamLeadDict = teamLeads.ToDictionary(tl => tl.Id);
            var juniorDict = juniors.ToDictionary(j => j.Id);

            var teamLeadQueue = new Queue<int>(teamLeads.Select(tl => tl.Id));

            var juniorPreferences = juniorsWishlists.ToDictionary(
                w => w.EmployeeId,
                w => (IEnumerable<int>)w.DesiredEmployees
            );

            var teamLeadPreferences = teamLeadsWishlists.ToDictionary(
                w => w.EmployeeId,
                w => new Queue<int>(w.DesiredEmployees)
            );

            var juniorAssignments = new Dictionary<int, int>();

            while (teamLeadQueue.Any())
            {
                int teamLeadId = teamLeadQueue.Dequeue();

                if (!teamLeadPreferences[teamLeadId].Any())
                {
                    continue;
                }

                int preferredJuniorId = teamLeadPreferences[teamLeadId].Dequeue();

                if (!juniorAssignments.TryGetValue(preferredJuniorId, out int currentTeamLeadId))
                {
                    juniorAssignments[preferredJuniorId] = teamLeadId;
                }
                else if (PrefersNewTeamLead(preferredJuniorId, teamLeadId, currentTeamLeadId, juniorPreferences))
                {
                    juniorAssignments[preferredJuniorId] = teamLeadId;
                    teamLeadQueue.Enqueue(currentTeamLeadId);
                }
                else
                {
                    teamLeadQueue.Enqueue(teamLeadId);
                }
            }

            return juniorAssignments.Select(engagement =>
                new Team(
                    teamLeadDict[engagement.Value],
                    juniorDict[engagement.Key]
                )
            ).ToList();
        }

        private static bool PrefersNewTeamLead(
            int juniorId,
            int newTeamLeadId,
            int currentTeamLeadId,
            Dictionary<int, IEnumerable<int>> juniorPreferences)
        {
            var preferences = juniorPreferences[juniorId].ToList();
            return preferences.IndexOf(newTeamLeadId) < preferences.IndexOf(currentTeamLeadId);
        }
    }
}
