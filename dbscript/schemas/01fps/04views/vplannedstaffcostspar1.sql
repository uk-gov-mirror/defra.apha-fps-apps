-- View: fps.vplannedstaffcostspar1
CREATE OR REPLACE VIEW fps.vplannedstaffcostspar1 AS
 SELECT vprojectstaffplan.parentproject,
    vprojectstaffplan.programno,
    vprojectstaffplan.fpsyear,
    sum(vprojectstaffplan.plannedhours) AS sumofplannedhours,
    sum(vprojectstaffplan.cost) AS sumofcost
   FROM (fps.vprojectstaffplan
     JOIN fps.tlkpproject ON (((vprojectstaffplan.parentproject)::text = (tlkpproject.parentproject)::text) AND vprojectstaffplan.fpsyear = tlkpproject.fpsyear))
  GROUP BY vprojectstaffplan.parentproject, vprojectstaffplan.programno, vprojectstaffplan.fpsyear;
