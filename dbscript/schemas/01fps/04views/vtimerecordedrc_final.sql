-- View: fps.vtimerecordedrc_final

CREATE OR REPLACE VIEW fps.vtimerecordedrc_final AS
 SELECT vtimerecordedrc.project,
    vtimerecordedrc.profitcentre,
    vtimerecordedrc.fpsyear,
    COALESCE(vplannedstaffcostssummary.sumofplannedhours, (0)::double precision) AS sumofplannedhours,
    COALESCE((vplannedstaffcostssummary.sumofcost)::numeric, (0)::numeric) AS sumofcost
   FROM (fps.vtimerecordedrc
     LEFT JOIN fps.vplannedstaffcostssummary ON ((((vtimerecordedrc.profitcentre)::text = (vplannedstaffcostssummary.profitcentre)::text) AND ((vtimerecordedrc.project)::text = (vplannedstaffcostssummary.parentproject)::text) AND (vtimerecordedrc.fpsyear = vplannedstaffcostssummary.fpsyear))))
  GROUP BY vtimerecordedrc.project, vtimerecordedrc.profitcentre, vtimerecordedrc.fpsyear, vplannedstaffcostssummary.sumofplannedhours, vplannedstaffcostssummary.sumofcost;
