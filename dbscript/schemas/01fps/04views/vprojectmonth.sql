-- View: fps.vprojectmonth

CREATE OR REPLACE VIEW fps.vprojectmonth AS
 SELECT projectmonth.project,
    projectmonth.monthno,
    projectmonth.costprofile,
    projectmonth.fpsyear
   FROM (fps.projectmonth
     JOIN fps.vtlkpproject ON ((((projectmonth.project)::text = (vtlkpproject.parentproject)::text) AND (vtlkpproject.fpsyear = projectmonth.fpsyear))));
