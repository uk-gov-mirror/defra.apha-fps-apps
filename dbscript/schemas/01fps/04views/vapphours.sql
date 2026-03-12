-- View: fps.vapphours
CREATE OR REPLACE VIEW fps.vapphours AS
 SELECT tblwgemployee.workgroupgrade,
    tblstaffjob.fpsyear,
    sum(tblstaffjob.plannedhours) AS sumofplannedhours
   FROM (((fps.tlkpproject
     JOIN fps.tblstaffjob ON (((tlkpproject.parentproject)::text = (tblstaffjob.jobcode)::text) AND tlkpproject.fpsyear = tblstaffjob.fpsyear))
     JOIN fps.tblwgemployee ON (((tblstaffjob.staffid)::text = (tblwgemployee.pactid)::text) AND tblstaffjob.fpsyear = tblwgemployee.fpsyear))
     JOIN fps.tlkpprogram ON (((tlkpproject.program)::text = (tlkpprogram.programno)::text) AND tlkpproject.fpsyear = tlkpprogram.fpsyear))
  WHERE (((tlkpproject.program)::text <> 'ZT_Prog'::text) AND ((tlkpproject.projectstatus)::text = 'approved'::text) AND ((tlkpprogram.sector_name)::text = 'Charge'::text))
  GROUP BY tblwgemployee.workgroupgrade, tblstaffjob.fpsyear;
