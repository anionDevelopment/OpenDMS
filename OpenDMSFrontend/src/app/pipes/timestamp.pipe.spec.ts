import { TimestampPipe } from './timestamp.pipe';

describe('TimestampPipe', () => {
  let pipe: TimestampPipe;

  beforeEach(() => {
    pipe = new TimestampPipe();
  });

  it('should return an empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('should return an empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('should return an empty string for an empty string', () => {
    expect(pipe.transform('')).toBe('');
  });

  it('should replace the date-time separator T with a space', () => {
    expect(pipe.transform('2024-01-02T10:20:30')).toBe('2024-01-02 10:20:30');
  });

  it('should insert a space before a positive UTC-offset', () => {
    expect(pipe.transform('2024-01-02T10:20:30+02:00')).toBe('2024-01-02 10:20:30 +02:00');
  });

  it('should insert a space before a negative UTC-offset', () => {
    expect(pipe.transform('2024-01-02T10:20:30-05:00')).toBe('2024-01-02 10:20:30 -05:00');
  });

  it('should not insert a space before a Z-suffix, because it is not matched by the offset-pattern', () => {
    expect(pipe.transform('2024-01-02T10:20:30Z')).toBe('2024-01-02 10:20:30Z');
  });
});
