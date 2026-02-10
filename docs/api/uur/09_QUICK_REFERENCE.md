## 9. Quick Reference

### 9.1 UUR Creation Decision Tree

```
Do you have a failed UUT report?
?? Yes ? Use CreateUURReport(operator, repairType, uutReport)
?  ?? Automatically links to UUT
?  ?? Copies part information
?  ?? Can link failures to test steps
?? No ? Use CreateUURReport(operator, repairType, opType, SN, PN, Rev)
   ?? Standalone repair
   ?? Manual part entry
   ?? Field repairs, customer returns
```

### 9.2 Common Mistakes

| Mistake | Solution |
|---------|----------|
| Not adding any failures | Add at least one failure |
| Invalid component reference | Check RepairType.ComponentReferenceMask |
| MiscInfo field not found | Check MiscInfo collection for available fields |
| Trying to add custom MiscInfo | MiscInfo is predefined by RepairType |
| Forgetting to set ExecutionTime | Track and set repair duration |

### 9.3 Validation Checklist

Before submitting a UUR report:

- [ ] At least one failure added
- [ ] All failures have valid fail codes
- [ ] Component references match mask (if defined)
- [ ] Required MiscInfo fields populated
- [ ] DateTime values valid (>= 1970-01-01)
- [ ] ExecutionTime set
- [ ] Comment added (recommended)
- [ ] Finalized timestamp set

### 9.4 UUR vs UUT Quick Comparison

| Feature | UUT Report | UUR Report |
|---------|-----------|-----------|
| Purpose | Testing | Repairing |
| Main Content | Steps & Measurements | Failures & Fail Codes |
| Structure | Hierarchical | Flat list |
| Fail Codes | No | Yes (hierarchical) |
| Status Propagation | Yes (in Active mode) | No |
| MiscInfo | User-defined | RepairType-defined |
| Reference | Can be standalone | Can reference UUT |

---

