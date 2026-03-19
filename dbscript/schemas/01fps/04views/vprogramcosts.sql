-- View: fps.vprogramcosts

CREATE OR REPLACE VIEW fps.vprogramcosts AS
 SELECT tlkpprogram.programno,
    tlkpproject.fpsyear,
    sum((projectmonthfinal.totalcost)::numeric) AS programcost
   FROM ((fps.tlkpprogram
     JOIN fps.tlkpproject ON ((((tlkpprogram.programno)::text = (tlkpproject.program)::text) AND (tlkpprogram.fpsyear = tlkpproject.fpsyear))))
     JOIN fps.projectmonthfinal ON ((((tlkpproject.parentproject)::text = (projectmonthfinal.project)::text) AND (tlkpproject.fpsyear = projectmonthfinal.fpsyear))))
  GROUP BY tlkpprogram.programno, tlkpproject.fpsyear;
