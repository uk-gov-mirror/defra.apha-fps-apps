-- View: fps.vplannedstaffcostssummary

CREATE OR REPLACE VIEW fps.vplannedstaffcostssummary AS
 SELECT workgroup.profitcentre,
    vprojectstaffplan.parentproject,
    vprojectstaffplan.fpsyear,
    sum(vprojectstaffplan.cost) AS sumofcost,
    sum(vprojectstaffplan.plannedhours) AS sumofplannedhours
   FROM (fps.vprojectstaffplan
     JOIN fps.workgroup ON ((((vprojectstaffplan.workgroup)::text = (workgroup.workgroup)::text) AND (vprojectstaffplan.fpsyear = workgroup.fpsyear))))
  GROUP BY workgroup.profitcentre, vprojectstaffplan.parentproject, vprojectstaffplan.fpsyear;
