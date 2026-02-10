## 8. Quick Reference

### 8.1 Step Type Decision Tree

```
Need to record measurement?
?? Yes ? What type of measurement?
?  ?? Numeric value vs limits
?  ?  ?? Use NumericLimitStep
?  ?? Boolean pass/fail
?  ?  ?? Use PassFailStep
?  ?? String comparison
?     ?? Use StringValueStep
?? No ? What is the purpose?
   ?? Organize sub-steps
   ?  ?? Use SequenceCall
   ?? Action/statement
      ?? Use GenericStep
```

### 8.2 Common Mistakes

| Mistake | Solution |
|---------|----------|
| Mixing single/multiple tests | Choose one type per step |
| Forgetting status in Import mode | Always set status explicitly |
| Exceeding string lengths | Use AutoTruncate or check lengths |
| Adding >1 chart per step | Only one chart allowed |
| Adding >1 attachment per step | Only one attachment allowed |
| Missing required properties | Validate before submit |
| Invalid datetime | Use dates >= 1970-01-01 |

### 8.3 Status Propagation Rules

| Mode | Auto-Propagation | Manual Control |
|------|------------------|----------------|
| **Active** | ? Failed/Error/Terminated | ? Not needed |
| **Import** | ? Disabled | ? Required |
| **TestStand** | ? Like Active | ? Not needed |

### 8.4 Validation Checklist

Before submitting a report:

- [ ] All required header properties set
- [ ] DateTime values valid (>= 1970-01-01)
- [ ] String lengths within limits
- [ ] All steps have status
- [ ] Report has final status
- [ ] No invalid XML characters
- [ ] Single/multiple test rules followed
- [ ] Charts/attachments not exceeding limits

---

