-- View: fps.qryfrmtimesellerpc_map
CREATE OR REPLACE VIEW fps.qryfrmtimesellerpc_map AS
 SELECT tblkpprofitcentre.conttarget,
    profitcentregrade.profitcentre AS sellingpc,
    profitcentregrade.chargerate,
    profitcentregrade.ohr,
    vqrytbidsum.sumofgenbid,
    workgroupgrade.workgroup,
    workgroupgrade.profitcentregrade,
    workgroupgrade.wggrade,
    vapphours.sumofplannedhours AS apphours,
    sum(vstaffjobhours.plannedhours) AS hrs,
    sum(tblwgemployee.hrsavail) AS avhrs,
    (sum(vstaffjobhours.plannedhours) * profitcentregrade.chargerate) AS fec,
    (vapphours.sumofplannedhours * profitcentregrade.chargerate) AS appfec,
    (profitcentregrade.ohr * sum(vstaffjobhours.plannedhours)) AS contribution,
    tblwgemployee.fpsyear
   FROM ((fps.vapphours
     RIGHT JOIN (((fps.tblkpprofitcentre
     JOIN (fps.profitcentregrade
     LEFT JOIN fps.vqrytbidsum ON (((profitcentregrade.profitcentre)::text = (vqrytbidsum.profitcentre)::text) AND profitcentregrade.fpsyear = vqrytbidsum.fpsyear)) ON (((tblkpprofitcentre.profitcentre)::text = (profitcentregrade.profitcentre)::text)))
     JOIN fps.workgroupgrade ON (((profitcentregrade.pcgrade)::text = (workgroupgrade.profitcentregrade)::text) AND profitcentregrade.fpsyear = workgroupgrade.fpsyear))
     JOIN fps.tblwgemployee ON (((workgroupgrade.wggrade)::text = (tblwgemployee.workgroupgrade)::text) AND workgroupgrade.fpsyear = tblwgemployee.fpsyear)) ON (((vapphours.workgroupgrade)::text = (workgroupgrade.wggrade)::text) AND vapphours.fpsyear = workgroupgrade.fpsyear))
     LEFT JOIN fps.vstaffjobhours ON (((tblwgemployee.pactid)::text = (vstaffjobhours.staffid)::text) AND tblwgemployee.fpsyear = vstaffjobhours.fpsyear))
  GROUP BY tblkpprofitcentre.conttarget, profitcentregrade.profitcentre, profitcentregrade.chargerate, profitcentregrade.ohr, vqrytbidsum.sumofgenbid, workgroupgrade.workgroup, workgroupgrade.profitcentregrade, workgroupgrade.wggrade, vapphours.sumofplannedhours, tblwgemployee.fpsyear;
