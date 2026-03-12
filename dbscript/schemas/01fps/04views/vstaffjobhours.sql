-- View: fps.vstaffjobhours
CREATE OR REPLACE VIEW fps.vstaffjobhours AS
 SELECT tblstaffjob.staffid,
    tblstaffjob.fpsyear,
    sum(tblstaffjob.plannedhours) AS plannedhours
   FROM ((fps.tblstaffjob
     JOIN fps.tlkpproject ON (((tblstaffjob.jobcode)::text = (tlkpproject.parentproject)::text) AND tblstaffjob.fpsyear = tlkpproject.fpsyear))
     JOIN fps.tlkpprogram ON (((tlkpproject.program)::text = (tlkpprogram.programno)::text) AND tlkpproject.fpsyear = tlkpprogram.fpsyear))
  WHERE (((tlkpproject.program)::text <> 'zt_prog'::text) AND ((tlkpprogram.sector_name)::text = 'Charge'::text))
  GROUP BY tblstaffjob.staffid, tblstaffjob.fpsyear;
