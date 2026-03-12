-- View: fps.vpactworkgroupgrade

CREATE OR REPLACE VIEW fps.vpactworkgroupgrade AS
 SELECT wggrade AS wg_grade,
    profitcentregrade,
    gradecode,
    workgroup,
    chargeratewg AS chargerate_wg,
    directratewg AS directrate_wg,
    payratewg AS payrate_wg,
    nprwg AS npr_wg,
    ohrwg AS ohr_wg,
    avsalary,
    hrschangedby,
    fpsyear
   FROM fps.workgroupgrade;
