-- View: fps.vmilestone

CREATE OR REPLACE VIEW fps.vmilestone AS
 SELECT milestone.project,
    milestone.milestoneref,
    milestone.objectiveref,
    milestone.milsetonetitle,
    milestone.plandate,
    milestone.actualdate,
    milestone.comment,
    milestone.monthnofin,
    milestone.year,
    milestone.fpsyear
   FROM (fps.milestone
     JOIN fps.vtlkpproject ON ((((milestone.project)::text = (vtlkpproject.parentproject)::text) AND (vtlkpproject.fpsyear = milestone.fpsyear))));
