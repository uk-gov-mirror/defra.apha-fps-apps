-- View: fps.vprojectstaffplan

CREATE OR REPLACE VIEW fps.vprojectstaffplan AS
 SELECT tlkpproject.parentproject,
    tlkpprogram.programno,
    tlkpproject.contract,
    (((COALESCE(tblemployee.lastname, ''::character varying))::text || ', '::text) || (COALESCE(tblemployee.firstname, ''::character varying))::text) AS name,
    tblstaffjob.staffid,
    tblstaffjob.plannedhours,
        CASE tlkpproject.isdefraproject
            WHEN 0 THEN profitcentregrade.chargerate
            ELSE profitcentregrade.defrachargerate
        END AS chargerate,
    ((tblstaffjob.plannedhours * (
        CASE tlkpprogram.sector_name
            WHEN 'charge'::text THEN (1)::numeric
            ELSE (0)::numeric
        END)::double precision) *
        CASE tlkpproject.isdefraproject
            WHEN 0 THEN profitcentregrade.chargerate
            ELSE profitcentregrade.defrachargerate
        END) AS cost,
    ((tblstaffjob.plannedhours * (
        CASE tlkpprogram.sector_name
            WHEN 'charge'::text THEN (1)::numeric
            ELSE (0)::numeric
        END)::double precision) * profitcentregrade.payrate) AS paycost,
    profitcentregrade.profitcentre,
    workgroupgrade.workgroup,
    workgroupgrade.wggrade,
    profitcentregrade.pcgrade,
    workgroupgrade.gradecode,
    tblwgemployee.fpsyear
   FROM ((((((fps.tblwgemployee
     JOIN fps.tblstaffjob ON ((((tblwgemployee.pactid)::text = (tblstaffjob.staffid)::text) AND (tblwgemployee.fpsyear = tblstaffjob.fpsyear))))
     JOIN fps.tblemployee ON (((tblwgemployee.spnumber)::text = (tblemployee.spnumber)::text)))
     JOIN fps.workgroupgrade ON ((((tblwgemployee.workgroupgrade)::text = (workgroupgrade.wggrade)::text) AND (tblwgemployee.fpsyear = workgroupgrade.fpsyear))))
     JOIN fps.profitcentregrade ON ((((workgroupgrade.profitcentregrade)::text = (profitcentregrade.pcgrade)::text) AND (workgroupgrade.fpsyear = profitcentregrade.fpsyear))))
     JOIN fps.tlkpproject ON ((((tblstaffjob.jobcode)::text = (tlkpproject.parentproject)::text) AND (tblstaffjob.fpsyear = tlkpproject.fpsyear))))
     JOIN fps.tlkpprogram ON ((((tlkpproject.program)::text = (tlkpprogram.programno)::text) AND (tlkpproject.fpsyear = tlkpprogram.fpsyear))));
